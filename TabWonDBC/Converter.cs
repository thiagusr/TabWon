// TabWonDBC — conversor de arquivos .dbc do DATASUS para Apache Parquet
// Copyright (C) 2026 Thiago Rodrigues
//
// Este programa é software livre: você pode redistribuí-lo e/ou modificá-lo sob os termos
// da Licença Pública Geral GNU (GNU GPL), versão 3, publicada pela Free Software Foundation.
//
// Este programa é distribuído na esperança de que seja útil, mas SEM NENHUMA GARANTIA,
// nem mesmo a garantia implícita de COMERCIALIZAÇÃO ou de ADEQUAÇÃO A UM FIM ESPECÍFICO.
// Veja a GNU GPL para mais detalhes. Uma cópia dela acompanha este programa no arquivo
// LICENSE; se não, veja <https://www.gnu.org/licenses/>.
//
// Termo adicional (GPL-3.0, seção 7, alínea b): este aviso, com o nome do programa original
// (TabWonDBC) e de seu autor (Thiago Rodrigues), deve ser mantido em todas as cópias e
// versões modificadas deste código-fonte. Veja o arquivo AVISO.md.
//
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TabWonDBC
{
    internal sealed class ConvertOptions
    {
        public int RowGroupSize = 1_000_000;  // 0 = um único row group (como a versão antiga)
        public int Threads = Environment.ProcessorCount;
        public bool Timing;
    }

    /// <summary>
    /// DBC -> Parquet em três estágios simultâneos:
    ///   1) descompactação (1 thread; o formato é sequencial por natureza)
    ///   2) conversão dos bytes dos registros em colunas tipadas (várias threads)
    ///   3) codificação/compressão das colunas (várias threads) e gravação do Parquet
    /// Nada passa pelo disco além do .dbc lido e do .parquet gravado.
    /// </summary>
    internal static class Converter
    {
        private const int Hist = BlastDecoder.Window;
        private const int TargetChunkBytes = 8 << 20; // ~8 MB de registros por bloco descompactado
        private const int RowBlock = 2048;            // linhas por sub-bloco (cabe no cache L2)

        private sealed class Chunk
        {
            public byte[] Buffer;  // fixado na memória (pinned)
            public int Rows;       // registros completos a partir de Hist
        }

        private sealed class Group
        {
            public ColumnData[] Columns;
            public int Count;
            public int Capacity;
        }

        private sealed class Stats
        {
            public long DecodeTicks, ParseTicks, WriteTicks;
            public long Records, Deleted;
        }

        public static unsafe void Convert(string dbcPath, string parquetPath, ConvertOptions opt)
        {
            // O .dbc é mapeado na memória: a descompactação começa já, e o sistema operacional
            // vai lendo o resto do arquivo do disco em paralelo (leitura antecipada).
            long fileLen = new FileInfo(dbcPath).Length;
            if (fileLen < 32) throw new InvalidDataException("Input stream too short");
            if (fileLen > int.MaxValue) throw new InvalidDataException("Arquivo .dbc maior que 2 GB.");
            using var mmf = MemoryMappedFile.CreateFromFile(dbcPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var view = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            byte* basePtr = null;
            view.SafeMemoryMappedViewHandle.AcquirePointer(ref basePtr);
            try
            {
                Run(dbcPath, (nint)(basePtr + view.PointerOffset), (int)fileLen, parquetPath, opt);
            }
            finally
            {
                view.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }

        private static unsafe void Run(string dbcPath, nint dbcPtr, int dbcLen, string parquetPath, ConvertOptions opt)
        {
            var st = new Stats();
            var total = Stopwatch.StartNew();

            var hdr = DbfHeader.Parse(new ReadOnlySpan<byte>((byte*)dbcPtr, dbcLen));
            int compStart = hdr.HeaderLength + 4; // cabeçalho DBF + 4 bytes de CRC
            if (dbcLen < compStart + 2) throw new InvalidDataException("Input stream too short");

            var fields = hdr.Fields;
            int nCols = fields.Count;
            int recLen = hdr.RecordLength;
            long totalRecords = hdr.RecordCount;
            int threads = Math.Max(1, opt.Threads);

            int rowsPerChunk = Math.Max(16, TargetChunkBytes / Math.Max(1, recLen));
            if (rowsPerChunk * recLen < 2 * Hist) rowsPerChunk = 2 * Hist / Math.Max(1, recLen) + 1;
            int bufSize = Hist + rowsPerChunk * recLen + BlastDecoder.MaxOverrun;

            int groupSize = opt.RowGroupSize <= 0
                ? (int)Math.Min(Math.Max(totalRecords, 1), Array.MaxLength)
                : opt.RowGroupSize;

            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var free = new BlockingCollection<byte[]>();
            for (int i = 0; i < 4; i++) free.Add(GC.AllocateUninitializedArray<byte>(bufSize, pinned: true));
            var chunks = new BlockingCollection<Chunk>(boundedCapacity: 2);
            var groups = new BlockingCollection<Group>(boundedCapacity: 1);
            using var groupSlots = new SemaphoreSlim(2); // no máximo 2 row groups na memória

            // ---------- estágio 1: descompactação ----------
            var decodeTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    var dec = new BlastDecoder((byte*)dbcPtr, compStart, dbcLen - compStart);
                    long remaining = totalRecords;
                    byte[] buf = free.Take(token);
                    int pos = Hist;
                    while (remaining > 0)
                    {
                        int rows = (int)Math.Min(rowsPerChunk, remaining);
                        int limit = Hist + rows * recLen;
                        long ts = Stopwatch.GetTimestamp();
                        pos = DecodeInto(dec, buf, pos, limit);
                        st.DecodeTicks += Stopwatch.GetTimestamp() - ts;

                        int got = Math.Min(rows, (pos - Hist) / recLen);
                        bool eof = got < rows; // os dados acabaram antes do total do cabeçalho
                        remaining -= got;
                        byte[] next = null;
                        if (!eof && remaining > 0)
                        {
                            // próximo buffer: janela (4 KB finais) + o que passou do limite
                            next = free.Take(token);
                            int end = Hist + got * recLen;
                            Buffer.BlockCopy(buf, end - Hist, next, 0, Hist);
                            int over = pos - end;
                            if (over > 0) Buffer.BlockCopy(buf, end, next, Hist, over);
                            pos = Hist + over;
                        }
                        if (got > 0) chunks.Add(new Chunk { Buffer = buf, Rows = got }, token);
                        else free.Add(buf);
                        if (next == null) break;
                        buf = next;
                    }
                }
                catch (OperationCanceledException) { }
                catch { cts.Cancel(); throw; }
                finally { chunks.CompleteAdding(); }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // ---------- estágio 3: codificação e gravação ----------
            var writeTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    using var fs = new FileStream(parquetPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20);
                    var writer = new PqFileWriter(fs, fields);
                    var po = new ParallelOptions { MaxDegreeOfParallelism = threads, CancellationToken = token };
                    var order = Enumerable.Range(0, nCols).OrderByDescending(c => fields[c].Length).ToArray();
                    foreach (var g in groups.GetConsumingEnumerable(token))
                    {
                        long ts = Stopwatch.GetTimestamp();
                        var enc = new EncodedChunk[nCols];
                        // colunas maiores primeiro, para equilibrar as threads
                        Parallel.ForEach(order, po, c => enc[c] = ChunkEncoder.Encode(g.Columns[c], fields[c], g.Count));
                        writer.WriteRowGroup(g.Count, enc);
                        g.Columns = null;
                        groupSlots.Release();
                        st.WriteTicks += Stopwatch.GetTimestamp() - ts;
                    }
                    writer.Finish();
                }
                catch (OperationCanceledException) { }
                catch { cts.Cancel(); throw; }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // ---------- estágio 2: conversão (nesta thread, com Parallel.For) ----------
            Exception parseError = null;
            try
            {
                var sets = PartitionColumns(fields, Math.Max(1, Math.Min(threads, nCols)));
                var po = new ParallelOptions { MaxDegreeOfParallelism = sets.Length, CancellationToken = token };
                var plain = new bool[nCols];  // colunas de texto com muitos valores distintos: sem dicionário
                int[] offs = new int[rowsPerChunk];
                long recordsLeft = totalRecords;
                Group cur = null;

                foreach (var ch in chunks.GetConsumingEnumerable(token))
                {
                    long ts = Stopwatch.GetTimestamp();
                    byte[] buf = ch.Buffer;
                    int m = 0;
                    for (int r = 0, o = Hist; r < ch.Rows; r++, o += recLen)
                        if (buf[o] != (byte)'*') offs[m++] = o; // registros apagados são ignorados (como no original)
                    st.Records += ch.Rows;
                    st.Deleted += ch.Rows - m;
                    recordsLeft -= ch.Rows - m;

                    int i = 0;
                    while (i < m)
                    {
                        if (cur == null)
                        {
                            st.ParseTicks += Stopwatch.GetTimestamp() - ts;
                            groupSlots.Wait(token);
                            ts = Stopwatch.GetTimestamp();
                            int cap = (int)Math.Min(groupSize, Math.Max(recordsLeft, 1));
                            cur = NewGroup(fields, plain, cap);
                        }
                        int take = Math.Min(m - i, cur.Capacity - cur.Count);
                        ParseRows(buf, offs, i, take, cur, fields, sets, po);
                        cur.Count += take;
                        recordsLeft -= take;
                        i += take;
                        if (cur.Count == cur.Capacity)
                        {
                            UpdatePlain(cur, plain);
                            st.ParseTicks += Stopwatch.GetTimestamp() - ts;
                            groups.Add(cur, token);
                            ts = Stopwatch.GetTimestamp();
                            cur = null;
                        }
                    }
                    free.Add(buf);
                    st.ParseTicks += Stopwatch.GetTimestamp() - ts;
                }
                if (cur != null)
                {
                    if (cur.Count > 0) groups.Add(cur, token);
                    else groupSlots.Release();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { parseError = ex; cts.Cancel(); }
            finally { groups.CompleteAdding(); }

            // espera os outros estágios e propaga o primeiro erro real
            var errors = new List<Exception>();
            try { decodeTask.GetAwaiter().GetResult(); } catch (OperationCanceledException) { } catch (Exception ex) { errors.Add(ex); }
            if (parseError != null) errors.Add(parseError);
            try { writeTask.GetAwaiter().GetResult(); } catch (OperationCanceledException) { } catch (Exception ex) { errors.Add(ex); }
            if (errors.Count > 0)
            {
                try { File.Delete(parquetPath); } catch { }
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(errors[0]).Throw();
            }

            if (opt.Timing)
            {
                double f = 1000.0 / Stopwatch.Frequency;
                Console.Error.WriteLine(
                    $"[tempo] {Path.GetFileName(dbcPath)}: total {total.ElapsedMilliseconds} ms | " +
                    $"descompactação {st.DecodeTicks * f:F0} ms | conversão {st.ParseTicks * f:F0} ms | gravação {st.WriteTicks * f:F0} ms | " +
                    $"{st.Records - st.Deleted:N0} registros ({st.Deleted:N0} apagados) | {nCols} colunas");
            }
        }

        private static unsafe int DecodeInto(BlastDecoder dec, byte[] buf, int pos, int limit)
        {
            fixed (byte* o = buf) return dec.Decode(o, pos, limit);
        }

        private static Group NewGroup(List<DbfField> fields, bool[] plain, int cap)
        {
            var g = new Group { Capacity = cap, Columns = new ColumnData[fields.Count] };
            for (int c = 0; c < fields.Count; c++)
            {
                var f = fields[c];
                ColumnData col = f.Kind switch
                {
                    ColKind.Int64 => new Int64Column(),
                    ColKind.Double => new DoubleColumn(),
                    ColKind.DateTime => new DateColumn(),
                    ColKind.Bool => new BoolColumn(),
                    _ => plain[c] ? new PlainStringColumn(f.Length) : new DictStringColumn(),
                };
                col.Prepare(cap);
                g.Columns[c] = col;
            }
            return g;
        }

        /// <summary>Se mais da metade dos valores de um row group foi distinta, o dicionário não compensa.</summary>
        private static void UpdatePlain(Group g, bool[] plain)
        {
            if (g.Count < 20_000) return;
            for (int c = 0; c < plain.Length; c++)
                if (g.Columns[c] is DictStringColumn d && d.Count > g.Count / 2) plain[c] = true;
        }

        /// <summary>Divide as colunas entre as threads equilibrando um custo aproximado.</summary>
        private static int[][] PartitionColumns(List<DbfField> fields, int n)
        {
            var load = new double[n];
            var sets = Enumerable.Range(0, n).Select(_ => new List<int>()).ToArray();
            foreach (int c in Enumerable.Range(0, fields.Count).OrderByDescending(c => Cost(fields[c])))
            {
                int best = 0;
                for (int k = 1; k < n; k++) if (load[k] < load[best]) best = k;
                sets[best].Add(c);
                load[best] += Cost(fields[c]);
            }
            foreach (var s in sets) s.Sort(); // ordem do registro = acesso mais sequencial
            return sets.Where(s => s.Count > 0).Select(s => s.ToArray()).ToArray();

            static double Cost(DbfField f) => f.Kind switch
            {
                ColKind.String => 2 + f.Length * 0.15,
                ColKind.Double => 3 + f.Length * 0.1,
                ColKind.Int64 => 1.5 + f.Length * 0.1,
                ColKind.DateTime => 2,
                _ => 1,
            };
        }

        private static unsafe void ParseRows(byte[] buf, int[] offs, int from, int count, Group g,
            List<DbfField> fields, int[][] sets, ParallelOptions po)
        {
            int dst0 = g.Count;
            Parallel.For(0, sets.Length, po, si =>
            {
                int[] set = sets[si];
                fixed (byte* b = buf)
                fixed (int* off = offs)
                {
                    for (int blk = 0; blk < count; blk += RowBlock)
                    {
                        int n = Math.Min(RowBlock, count - blk);
                        int* ro = off + from + blk;
                        int d = dst0 + blk;
                        foreach (int c in set)
                        {
                            var f = fields[c];
                            byte* bp = b + f.Offset;
                            int w = f.Length;
                            switch (g.Columns[c])
                            {
                                case DictStringColumn col:
                                {
                                    int[] ids = col.Ids;
                                    if (w <= 16)
                                    {
                                        // lê 16 bytes a partir do campo: o buffer tem folga no fim
                                        ulong m0 = w >= 8 ? ulong.MaxValue : (1UL << (8 * w)) - 1;
                                        ulong m1 = w <= 8 ? 0 : w == 16 ? ulong.MaxValue : (1UL << (8 * (w - 8))) - 1;
                                        for (int r = 0; r < n; r++) ids[d + r] = col.GetIdRaw(bp + ro[r], w, m0, m1);
                                    }
                                    else
                                    {
                                        for (int r = 0; r < n; r++)
                                        {
                                            byte* p = bp + ro[r];
                                            int len = Cell.Trim(p, w, out int s);
                                            ids[d + r] = col.GetId(p + s, len);
                                        }
                                    }
                                    break;
                                }
                                case PlainStringColumn col:
                                {
                                    for (int r = 0; r < n; r++)
                                    {
                                        byte* p = bp + ro[r];
                                        int len = Cell.Trim(p, w, out int s);
                                        col.Append(d + r, p + s, len);
                                    }
                                    break;
                                }
                                case Int64Column col:
                                {
                                    long[] arr = col.Values;
                                    for (int r = 0; r < n; r++) arr[d + r] = Cell.ParseInt64(bp + ro[r], w);
                                    break;
                                }
                                case DoubleColumn col:
                                {
                                    double[] arr = col.Values;
                                    for (int r = 0; r < n; r++) arr[d + r] = Cell.ParseDouble(bp + ro[r], w);
                                    break;
                                }
                                case DateColumn col:
                                {
                                    DateTime[] arr = col.Values;
                                    for (int r = 0; r < n; r++) arr[d + r] = Cell.ParseDate(bp + ro[r], w);
                                    break;
                                }
                                case BoolColumn col:
                                {
                                    bool[] arr = col.Values;
                                    for (int r = 0; r < n; r++) arr[d + r] = Cell.ParseBool(bp + ro[r], w);
                                    break;
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}

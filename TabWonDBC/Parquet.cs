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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Snappier;

namespace TabWonDBC
{
    /// <summary>
    /// Gravador Parquet mínimo para tabelas planas (sem aninhamento), com o mesmo schema que
    /// o Parquet.Net 4 gerava na versão antiga:
    ///   texto   -> optional BYTE_ARRAY (UTF8)     (com dicionário quando compensa)
    ///   inteiro -> required INT64 (INT_64)
    ///   decimal -> required DOUBLE
    ///   data    -> required INT96
    ///   lógico  -> required BOOLEAN
    /// Compressão Snappy. Cada coluna de um row group é codificada em paralelo.
    /// </summary>
    internal sealed class PqFileWriter : IDisposable
    {
        private readonly Stream _out;
        private readonly List<DbfField> _fields;
        private readonly List<RowGroupMeta> _groups = new();
        private long _pos;
        private long _rows;

        public const int PageRows = 128 * 1024;

        internal sealed class ChunkMeta
        {
            public int Type;               // tipo físico Parquet
            public int[] Encodings;
            public long NumValues;
            public long Uncompressed, Compressed;
            public long DictPageOffset = -1, DataPageOffset; // relativos ao início do chunk até serem gravados
            public byte[] Min, Max;
            public bool DataStarted;
        }

        private sealed class RowGroupMeta
        {
            public long NumRows, TotalByteSize, TotalCompressed, FileOffset;
            public ChunkMeta[] Chunks;
            public long[] ChunkOffsets;
        }

        public PqFileWriter(Stream output, List<DbfField> fields)
        {
            _out = output;
            _fields = fields;
            _out.Write("PAR1"u8);
            _pos = 4;
        }

        /// <summary>Grava um row group já codificado (as colunas são gravadas na ordem).</summary>
        public void WriteRowGroup(int rows, EncodedChunk[] chunks)
        {
            var g = new RowGroupMeta { NumRows = rows, Chunks = new ChunkMeta[chunks.Length], ChunkOffsets = new long[chunks.Length], FileOffset = _pos };
            for (int c = 0; c < chunks.Length; c++)
            {
                var ch = chunks[c];
                long start = _pos;
                _out.Write(ch.Buffer, 0, ch.Length);
                _pos += ch.Length;
                ch.Meta.DataPageOffset += start;
                if (ch.Meta.DictPageOffset >= 0) ch.Meta.DictPageOffset += start;
                g.Chunks[c] = ch.Meta;
                g.ChunkOffsets[c] = start;
                g.TotalByteSize += ch.Meta.Uncompressed;
                g.TotalCompressed += ch.Meta.Compressed;
            }
            _groups.Add(g);
            _rows += rows;
        }

        public void Finish()
        {
            var t = new Thrift();
            // FileMetaData
            t.StructBegin();
            t.I32(1, 1);                                   // version
            t.ListBegin(2, Thrift.TStruct, _fields.Count + 1); // schema
            {
                t.StructBegin();
                t.I32(3, 0);                               // REQUIRED
                t.Str(4, "root");
                t.I32(5, _fields.Count);
                t.StructEnd();
                foreach (var f in _fields)
                {
                    t.StructBegin();
                    t.I32(1, PhysicalType(f.Kind));
                    t.I32(3, f.Kind == ColKind.String ? 1 : 0); // OPTIONAL só para texto (como antes)
                    t.Str(4, f.Name);
                    if (f.Kind == ColKind.String) t.I32(6, 0);   // ConvertedType UTF8
                    if (f.Kind == ColKind.Int64) t.I32(6, 18);   // ConvertedType INT_64
                    if (f.Kind == ColKind.String)
                    {
                        t.StructFieldBegin(10); t.StructFieldBegin(1); t.StructEnd(); t.StructEnd(); // LogicalType.STRING
                    }
                    else if (f.Kind == ColKind.Int64)
                    {
                        t.StructFieldBegin(10); t.StructFieldBegin(10); t.I8(1, 64); t.Bool(2, true); t.StructEnd(); t.StructEnd(); // INTEGER(64, signed)
                    }
                    t.StructEnd();
                }
            }
            t.I64(3, _rows);
            t.ListBegin(4, Thrift.TStruct, _groups.Count);
            for (int gi = 0; gi < _groups.Count; gi++)
            {
                var g = _groups[gi];
                t.StructBegin();
                t.ListBegin(1, Thrift.TStruct, g.Chunks.Length);
                for (int c = 0; c < g.Chunks.Length; c++)
                {
                    var m = g.Chunks[c];
                    t.StructBegin();
                    t.I64(2, g.ChunkOffsets[c]);           // file_offset
                    t.StructFieldBegin(3);                 // ColumnMetaData
                    t.I32(1, m.Type);
                    t.ListBegin(2, Thrift.TI32, m.Encodings.Length);
                    foreach (int e in m.Encodings) t.ListI32(e);
                    t.ListBegin(3, Thrift.TBinary, 1);
                    t.ListStr(_fields[c].Name);
                    t.I32(4, 1);                           // SNAPPY
                    t.I64(5, m.NumValues);
                    t.I64(6, m.Uncompressed);
                    t.I64(7, m.Compressed);
                    t.I64(9, m.DataPageOffset);
                    if (m.DictPageOffset >= 0) t.I64(11, m.DictPageOffset);
                    t.StructFieldBegin(12);                // Statistics
                    t.I64(3, 0);                           // null_count
                    if (m.Max != null) { t.Bin(5, m.Max); t.Bin(6, m.Min); }
                    t.StructEnd();
                    t.StructEnd();                         // ColumnMetaData
                    t.StructEnd();                         // ColumnChunk
                }
                t.I64(2, g.TotalByteSize);
                t.I64(3, g.NumRows);
                t.I64(5, g.FileOffset);
                t.I64(6, g.TotalCompressed);
                t.I16(7, (short)gi);
                t.StructEnd();
            }
            t.Str(6, "TabWonDBC 2.0 (formato compatível com Parquet.Net 4)");
            t.StructEnd();

            _out.Write(t.Buffer, 0, t.Length);
            Span<byte> tail = stackalloc byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(tail, t.Length);
            "PAR1"u8.CopyTo(tail.Slice(4));
            _out.Write(tail);
            _out.Flush();
        }

        public void Dispose() { }

        public static int PhysicalType(ColKind k) => k switch
        {
            ColKind.Bool => 0,      // BOOLEAN
            ColKind.Int64 => 2,     // INT64
            ColKind.DateTime => 3,  // INT96
            ColKind.Double => 5,    // DOUBLE
            _ => 6,                 // BYTE_ARRAY
        };
    }

    internal sealed class EncodedChunk
    {
        public byte[] Buffer;
        public int Length;
        public PqFileWriter.ChunkMeta Meta;
    }

    /// <summary>Codifica uma coluna de um row group em páginas Parquet comprimidas.</summary>
    internal static unsafe class ChunkEncoder
    {
        private const int EncPlain = 0, EncPlainDict = 2, EncRle = 3;

        private sealed class Out
        {
            public byte[] Buf = new byte[1 << 16];
            public int Len;
            public void Ensure(int extra)
            {
                if (Len + extra > Buf.Length)
                    Array.Resize(ref Buf, (int)Math.Min(Math.Max((long)Buf.Length * 2, (long)Len + extra), Array.MaxLength));
            }
            public void Write(ReadOnlySpan<byte> s) { Ensure(s.Length); s.CopyTo(Buf.AsSpan(Len)); Len += s.Length; }
        }

        [ThreadStatic] private static byte[] _page;
        [ThreadStatic] private static byte[] _comp;

        private static byte[] PageBuf(int size)
        {
            if (_page == null || _page.Length < size) _page = GC.AllocateUninitializedArray<byte>(Math.Max(size, 1 << 20));
            return _page;
        }

        public static EncodedChunk Encode(ColumnData col, DbfField f, int rows)
        {
            var o = new Out();
            var m = new PqFileWriter.ChunkMeta { Type = PqFileWriter.PhysicalType(f.Kind), NumValues = rows };
            switch (col)
            {
                case DictStringColumn d: EncodeDict(d, rows, o, m); break;
                case PlainStringColumn p: EncodePlainStrings(p, rows, o, m); break;
                case Int64Column l: EncodeFixed(MemoryMarshal.AsBytes(l.Values.AsSpan(0, rows)), 8, rows, o, m); Stats(l.Values, rows, m); break;
                case DoubleColumn d: EncodeFixed(MemoryMarshal.AsBytes(d.Values.AsSpan(0, rows)), 8, rows, o, m); Stats(d.Values, rows, m); break;
                case DateColumn dt: EncodeDates(dt.Values, rows, o, m); break;
                case BoolColumn b: EncodeBools(b.Values, rows, o, m); break;
                default: throw new InvalidOperationException();
            }
            return new EncodedChunk { Buffer = o.Buf, Length = o.Len, Meta = m };
        }

        // ---------- páginas ----------

        private static void WritePage(Out o, PqFileWriter.ChunkMeta m, bool dictionary, int numValues, int encoding,
                                      byte[] body, int bodyLen)
        {
            int max = Snappy.GetMaxCompressedLength(bodyLen);
            if (_comp == null || _comp.Length < max) _comp = GC.AllocateUninitializedArray<byte>(Math.Max(max, 1 << 20));
            int clen = Snappy.Compress(body.AsSpan(0, bodyLen), _comp);

            var t = new Thrift(64);
            t.StructBegin();
            t.I32(1, dictionary ? 2 : 0);   // DICTIONARY_PAGE / DATA_PAGE
            t.I32(2, bodyLen);
            t.I32(3, clen);
            if (dictionary)
            {
                t.StructFieldBegin(7);
                t.I32(1, numValues);
                t.I32(2, EncPlainDict);
                t.StructEnd();
            }
            else
            {
                t.StructFieldBegin(5);
                t.I32(1, numValues);
                t.I32(2, encoding);
                t.I32(3, EncRle);           // definition levels
                t.I32(4, EncRle);           // repetition levels
                t.StructEnd();
            }
            t.StructEnd();

            if (dictionary) m.DictPageOffset = o.Len;
            else if (!m.DataStarted) { m.DataPageOffset = o.Len; m.DataStarted = true; }
            o.Write(t.Buffer.AsSpan(0, t.Length));
            o.Write(_comp.AsSpan(0, clen));
            m.Uncompressed += t.Length + bodyLen;
            m.Compressed += t.Length + clen;
        }

        /// <summary>Níveis de definição para coluna opcional sem nulos: uma corrida RLE de 1s.</summary>
        private static int WriteAllDefined(byte* p, int n)
        {
            byte* q = p + 4;
            q += WriteVarint(q, (ulong)n << 1);
            *q++ = 1;
            int len = (int)(q - p - 4);
            BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(p, 4), len);
            return len + 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int WriteVarint(byte* p, ulong v)
        {
            int i = 0;
            while (v >= 0x80) { p[i++] = (byte)(v | 0x80); v >>= 7; }
            p[i++] = (byte)v;
            return i;
        }

        // ---------- texto com dicionário ----------

        private static void EncodeDict(DictStringColumn d, int rows, Out o, PqFileWriter.ChunkMeta m)
        {
            if (d.Count > rows / 2) { EncodeDictAsPlain(d, rows, o, m); return; } // dicionário não compensa
            m.Encodings = new[] { EncPlainDict, EncRle };
            // página de dicionário: valores PLAIN (tamanho de 4 bytes + UTF-8)
            int count = d.Count;
            long need = 0;
            fixed (byte* keys = d.Keys)
            {
                for (int i = 0; i < count; i++)
                {
                    int s = d.KeyStart[i], n = d.KeyStart[i + 1] - s;
                    need += 4 + Latin1.Utf8Length(keys + s, n);
                }
                byte[] page = PageBuf((int)need + 16);
                fixed (byte* pg = page)
                {
                    byte* q = pg;
                    for (int i = 0; i < count; i++)
                    {
                        int s = d.KeyStart[i], n = d.KeyStart[i + 1] - s;
                        int len = Latin1.ToUtf8(keys + s, n, q + 4);
                        BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(q, 4), len);
                        q += 4 + len;
                    }
                    WritePage(o, m, true, count, 0, page, (int)(q - pg));
                }
            }

            // páginas de dados: níveis de definição + largura de bits + índices bit-packed
            int bw = count <= 1 ? 1 : 32 - System.Numerics.BitOperations.LeadingZeroCount((uint)(count - 1));
            for (int start = 0; start < rows; start += PqFileWriter.PageRows)
            {
                int n = Math.Min(PqFileWriter.PageRows, rows - start);
                int groups = (n + 7) / 8;
                int size = 32 + 1 + 10 + groups * bw;
                byte[] page = PageBuf(size);
                fixed (byte* pg = page)
                fixed (int* ids = d.Ids)
                {
                    byte* q = pg + WriteAllDefined(pg, n);
                    *q++ = (byte)bw;
                    q += WriteVarint(q, ((ulong)groups << 1) | 1);
                    q = BitPack(ids + start, n, bw, q, groups * bw);
                    WritePage(o, m, false, n, EncPlainDict, page, (int)(q - pg));
                }
            }
        }

        private static byte* BitPack(int* v, int n, int bw, byte* q, int totalBytes)
        {
            byte* end = q + totalBytes;
            ulong acc = 0;
            int bits = 0;
            for (int i = 0; i < n; i++)
            {
                acc |= (ulong)(uint)v[i] << bits;
                bits += bw;
                if (bits >= 32)
                {
                    Unsafe.WriteUnaligned(q, (uint)acc);
                    q += 4; acc >>= 32; bits -= 32;
                }
            }
            while (bits > 0) { *q++ = (byte)acc; acc >>= 8; bits -= 8; }
            while (q < end) *q++ = 0;   // completa o último grupo de 8 valores
            return end;
        }

        // ---------- texto sem dicionário ----------

        private static void EncodeDictAsPlain(DictStringColumn d, int rows, Out o, PqFileWriter.ChunkMeta m)
        {
            m.Encodings = new[] { EncPlain, EncRle };
            fixed (byte* keys = d.Keys)
            fixed (int* ids = d.Ids)
            {
                for (int start = 0; start < rows; start += PqFileWriter.PageRows)
                {
                    int n = Math.Min(PqFileWriter.PageRows, rows - start);
                    long need = 32;
                    for (int i = start; i < start + n; i++)
                    {
                        int id = ids[i];
                        need += 4 + 2L * (d.KeyStart[id + 1] - d.KeyStart[id]);
                    }
                    byte[] page = PageBuf((int)need);
                    fixed (byte* pg = page)
                    {
                        byte* q = pg + WriteAllDefined(pg, n);
                        for (int i = start; i < start + n; i++)
                        {
                            int id = ids[i], s = d.KeyStart[id];
                            int len = Latin1.ToUtf8(keys + s, d.KeyStart[id + 1] - s, q + 4);
                            BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(q, 4), len);
                            q += 4 + len;
                        }
                        WritePage(o, m, false, n, EncPlain, page, (int)(q - pg));
                    }
                }
            }
        }

        private static void EncodePlainStrings(PlainStringColumn p, int rows, Out o, PqFileWriter.ChunkMeta m)
        {
            m.Encodings = new[] { EncPlain, EncRle };
            for (int start = 0; start < rows; start += PqFileWriter.PageRows)
            {
                int n = Math.Min(PqFileWriter.PageRows, rows - start);
                int b0 = start == 0 ? 0 : p.Ends[start - 1];
                int b1 = p.Ends[start + n - 1];
                byte[] page = PageBuf(32 + (b1 - b0) + 4 * n);
                fixed (byte* pg = page)
                fixed (byte* data = p.Data)
                {
                    byte* q = pg + WriteAllDefined(pg, n);
                    int prev = b0;
                    for (int i = start; i < start + n; i++)
                    {
                        int e = p.Ends[i], len = e - prev;
                        BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(q, 4), len);
                        Buffer.MemoryCopy(data + prev, q + 4, len, len);
                        q += 4 + len;
                        prev = e;
                    }
                    WritePage(o, m, false, n, EncPlain, page, (int)(q - pg));
                }
            }
        }

        // ---------- tipos fixos ----------

        private static void EncodeFixed(ReadOnlySpan<byte> all, int size, int rows, Out o, PqFileWriter.ChunkMeta m)
        {
            m.Encodings = new[] { EncPlain };
            for (int start = 0; start < rows; start += PqFileWriter.PageRows)
            {
                int n = Math.Min(PqFileWriter.PageRows, rows - start);
                byte[] page = PageBuf(n * size);
                all.Slice(start * size, n * size).CopyTo(page);
                WritePage(o, m, false, n, EncPlain, page, n * size);
            }
            if (rows == 0) WritePage(o, m, false, 0, EncPlain, PageBuf(16), 0);
        }

        private static void EncodeDates(DateTime[] v, int rows, Out o, PqFileWriter.ChunkMeta m)
        {
            m.Encodings = new[] { EncPlain };
            const long UnixEpochJulian = 2440588;
            long epochDays = new DateTime(1970, 1, 1).Ticks / TimeSpan.TicksPerDay;
            for (int start = 0; start < rows || (rows == 0 && start == 0); start += PqFileWriter.PageRows)
            {
                int n = Math.Min(PqFileWriter.PageRows, rows - start);
                byte[] page = PageBuf(n * 12 + 16);
                fixed (byte* pg = page)
                {
                    byte* q = pg;
                    for (int i = start; i < start + n; i++)
                    {
                        long ticks = v[i].Ticks;
                        long day = ticks / TimeSpan.TicksPerDay;
                        long nanos = (ticks - day * TimeSpan.TicksPerDay) * 100;
                        Unsafe.WriteUnaligned(q, nanos);
                        Unsafe.WriteUnaligned(q + 8, (int)(day - epochDays + UnixEpochJulian));
                        q += 12;
                    }
                }
                WritePage(o, m, false, n, EncPlain, page, n * 12);
                if (rows == 0) break;
            }
        }

        private static void EncodeBools(bool[] v, int rows, Out o, PqFileWriter.ChunkMeta m)
        {
            m.Encodings = new[] { EncPlain };
            for (int start = 0; start < rows || (rows == 0 && start == 0); start += PqFileWriter.PageRows)
            {
                int n = Math.Min(PqFileWriter.PageRows, rows - start);
                int bytes = (n + 7) / 8;
                byte[] page = PageBuf(bytes + 16);
                Array.Clear(page, 0, bytes);
                for (int i = 0; i < n; i++) if (v[start + i]) page[i >> 3] |= (byte)(1 << (i & 7));
                WritePage(o, m, false, n, EncPlain, page, bytes);
                if (rows == 0) break;
            }
        }

        // ---------- estatísticas (mín/máx) ----------

        private static void Stats(long[] v, int rows, PqFileWriter.ChunkMeta m)
        {
            if (rows == 0) return;
            long mn = long.MaxValue, mx = long.MinValue;
            for (int i = 0; i < rows; i++) { long x = v[i]; if (x < mn) mn = x; if (x > mx) mx = x; }
            m.Min = BitConverter.GetBytes(mn);
            m.Max = BitConverter.GetBytes(mx);
        }

        private static void Stats(double[] v, int rows, PqFileWriter.ChunkMeta m)
        {
            if (rows == 0) return;
            double mn = double.PositiveInfinity, mx = double.NegativeInfinity;
            for (int i = 0; i < rows; i++) { double x = v[i]; if (x < mn) mn = x; if (x > mx) mx = x; }
            if (mn == 0) mn = -0.0;   // recomendação da especificação para zeros
            if (mx == 0) mx = 0.0;
            m.Min = BitConverter.GetBytes(mn);
            m.Max = BitConverter.GetBytes(mx);
        }
    }

    /// <summary>Thrift "compact protocol" — só o necessário para os metadados do Parquet.</summary>
    internal sealed class Thrift
    {
        public const byte TBoolTrue = 1, TBoolFalse = 2, TByte = 3, TI16 = 4, TI32 = 5, TI64 = 6, TBinary = 8, TList = 9, TStruct = 12;

        public byte[] Buffer;
        public int Length;
        private readonly Stack<short> _stack = new();
        private short _last;

        public Thrift(int capacity = 4096) { Buffer = new byte[capacity]; }

        private void Byte(byte b)
        {
            if (Length == Buffer.Length) Array.Resize(ref Buffer, Buffer.Length * 2);
            Buffer[Length++] = b;
        }

        private void Varint(ulong v)
        {
            while (v >= 0x80) { Byte((byte)(v | 0x80)); v >>= 7; }
            Byte((byte)v);
        }

        private static ulong Zig(long v) => (ulong)((v << 1) ^ (v >> 63));

        private void Field(byte type, short id)
        {
            int delta = id - _last;
            if (delta > 0 && delta <= 15) Byte((byte)((delta << 4) | type));
            else { Byte(type); Varint(Zig(id)); }
            _last = id;
        }

        public void StructBegin() { _stack.Push(_last); _last = 0; }
        public void StructEnd() { Byte(0); _last = _stack.Pop(); }
        public void StructFieldBegin(short id) { Field(TStruct, id); StructBegin(); }
        public void I8(short id, sbyte v) { Field(TByte, id); Byte((byte)v); }
        public void I16(short id, short v) { Field(TI16, id); Varint(Zig(v)); }
        public void I32(short id, int v) { Field(TI32, id); Varint(Zig(v)); }
        public void I64(short id, long v) { Field(TI64, id); Varint(Zig(v)); }
        public void Bool(short id, bool v) => Field(v ? TBoolTrue : TBoolFalse, id);
        public void Bin(short id, byte[] v) { Field(TBinary, id); Raw(v); }
        public void Str(short id, string s) { Field(TBinary, id); Raw(Encoding.UTF8.GetBytes(s)); }

        public void ListBegin(short id, byte elemType, int count)
        {
            Field(TList, id);
            if (count < 15) Byte((byte)((count << 4) | elemType));
            else { Byte((byte)(0xF0 | elemType)); Varint((ulong)count); }
        }
        public void ListI32(int v) => Varint(Zig(v));
        public void ListStr(string s) => Raw(Encoding.UTF8.GetBytes(s));

        private void Raw(byte[] b)
        {
            Varint((ulong)b.Length);
            if (Length + b.Length > Buffer.Length) Array.Resize(ref Buffer, Math.Max(Buffer.Length * 2, Length + b.Length));
            b.CopyTo(Buffer, Length);
            Length += b.Length;
        }
    }
}

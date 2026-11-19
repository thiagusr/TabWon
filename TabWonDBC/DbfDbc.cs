// TabWonDBC — conversor de arquivos .dbc do DATASUS para Apache Parquet
// Copyright (C) 2026 Thiago Rodrigues (thiagusr)
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
// (TabWonDBC) e de seu autor (Thiago Rodrigues - thiagusr), deve ser mantido em todas as cópias e
// versões modificadas deste código-fonte. Veja o arquivo AVISO.md.
//
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace TabWonDBC
{
    /// <summary>
    /// CRC-32 (o mesmo do zip/zlib), usado no cabeçalho do DBC: é o CRC do arquivo DBF inteiro
    /// (cabeçalho + registros + byte final 0x1A).
    /// </summary>
    internal static class Crc32
    {
        private static readonly uint[] T = Build();

        private static uint[] Build()
        {
            var t = new uint[8 * 256];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
                t[i] = c;
            }
            for (int i = 0; i < 256; i++)
                for (int s = 1; s < 8; s++)
                    t[s * 256 + i] = (t[(s - 1) * 256 + i] >> 8) ^ t[t[(s - 1) * 256 + i] & 0xFF];
            return t;
        }

        /// <summary>Atualiza o CRC (começar com 0). Processa 8 bytes por vez (slicing-by-8).</summary>
        public static uint Update(uint crc, ReadOnlySpan<byte> d)
        {
            crc = ~crc;
            int i = 0;
            for (; i + 8 <= d.Length; i += 8)
            {
                uint a = crc ^ BinaryPrimitives.ReadUInt32LittleEndian(d.Slice(i));
                uint b = BinaryPrimitives.ReadUInt32LittleEndian(d.Slice(i + 4));
                crc = T[7 * 256 + (a & 0xFF)] ^ T[6 * 256 + ((a >> 8) & 0xFF)] ^ T[5 * 256 + ((a >> 16) & 0xFF)] ^
                      T[4 * 256 + (a >> 24)] ^ T[3 * 256 + (b & 0xFF)] ^ T[2 * 256 + ((b >> 8) & 0xFF)] ^
                      T[1 * 256 + ((b >> 16) & 0xFF)] ^ T[b >> 24];
            }
            for (; i < d.Length; i++) crc = T[(crc ^ d[i]) & 0xFF] ^ (crc >> 8);
            return ~crc;
        }
    }

    /// <summary>
    /// Conversões diretas entre DBC (DATASUS) e DBF (dBase).
    /// Formato do DBC: cabeçalho DBF idêntico ao original, 4 bytes com o CRC-32 do DBF inteiro
    /// (little-endian) e, em seguida, os registros comprimidos com PKWare DCL "implode"
    /// (literais sem código, dicionário de 4 KB).
    /// </summary>
    internal static unsafe class DbfDbc
    {
        private const int Hist = BlastDecoder.Window;
        private const int Chunk = 4 << 20;

        // ------------------------------------------------------------------ DBC -> DBF
        /// <summary>Resultado da descompactação/verificação de um DBC.</summary>
        public sealed class Teste
        {
            public uint CrcGravado, CrcCalculado;
            public long Bytes, BytesEsperados;
            public uint Registros;
            public bool CrcOk => CrcGravado == 0 || CrcGravado == CrcCalculado;
            public bool TamanhoOk => Bytes == BytesEsperados || Bytes == BytesEsperados - 1;
        }

        /// <summary>
        /// Descompacta o DBC calculando o CRC-32; se <paramref name="saida"/> não for nulo, grava ali o DBF
        /// (com o terminador 0x0D no fim do cabeçalho).
        /// </summary>
        private static Teste Descompactar(string dbcPath, Stream? saida)
        {
            long len = new FileInfo(dbcPath).Length;
            if (len < 34) throw new InvalidDataException("arquivo DBC vazio ou truncado");
            if (len > int.MaxValue) throw new InvalidDataException("DBC maior que 2 GB não é suportado");
            var r = new Teste();
            using var mmf = MemoryMappedFile.CreateFromFile(dbcPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var view = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            byte* p = null;
            view.SafeMemoryMappedViewHandle.AcquirePointer(ref p);
            try
            {
                p += view.PointerOffset;
                int n = (int)len;
                var all = new ReadOnlySpan<byte>(p, n);
                var hdr = DbfHeader.Parse(all);
                int hl = hdr.HeaderLength;
                if (hl + 4 > n) throw new InvalidDataException("arquivo DBC truncado");
                r.Registros = hdr.RecordCount;
                r.BytesEsperados = (long)hdr.RecordCount * hdr.RecordLength + 1;
                r.CrcGravado = BinaryPrimitives.ReadUInt32LittleEndian(all.Slice(hl, 4));
                if (saida != null)
                {
                    var cab = all.Slice(0, hl).ToArray();
                    cab[hl - 1] = 0x0D;
                    saida.Write(cab);
                }
                uint crc = Crc32.Update(0, all.Slice(0, hl));
                var dec = new BlastDecoder(p, hl + 4, n - hl - 4);
                var buf = GC.AllocateUninitializedArray<byte>(Hist + Chunk + BlastDecoder.MaxOverrun, pinned: true);
                int pos = Hist;
                fixed (byte* o = buf)
                {
                    while (!dec.Finished)
                    {
                        int antes = pos;
                        pos = dec.Decode(o, pos, Hist + Chunk);
                        if (pos == antes && !dec.Finished) throw new InvalidDataException("dados comprimidos truncados");
                        var parte = new ReadOnlySpan<byte>(o + Hist, pos - Hist);
                        saida?.Write(parte);
                        crc = Crc32.Update(crc, parte);
                        r.Bytes += parte.Length;
                        Buffer.MemoryCopy(o + pos - Hist, o, Hist, Hist);
                        pos = Hist;
                    }
                }
                r.CrcCalculado = crc;
            }
            finally { view.SafeMemoryMappedViewHandle.ReleasePointer(); }
            return r;
        }

        public static void ParaDbf(string dbcPath, string dbfPath)
        {
            string tmp = dbfPath + ".tmp_tabwon";
            try
            {
                Teste r;
                using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20))
                    r = Descompactar(dbcPath, fs);
                if (!r.CrcOk)
                    Console.Error.WriteLine($"AVISO: CRC do DBC não confere (gravado {r.CrcGravado:X8}, calculado {r.CrcCalculado:X8}).");
                File.Move(tmp, dbfPath, true);
            }
            catch { try { File.Delete(tmp); } catch { } throw; }
        }

        /// <summary>Verifica a integridade do DBC sem gravar nada. Retorna 0 se íntegro, 3 se houver falha.</summary>
        public static int Testar(string dbcPath)
        {
            Teste r;
            try { r = Descompactar(dbcPath, null); }
            catch (Exception ex)
            {
                Console.WriteLine($"FALHA erro=\"{ex.Message}\"");
                return 3;
            }
            bool ok = r.CrcOk && r.TamanhoOk;
            Console.WriteLine($"{(ok ? "OK" : "FALHA")} registros={r.Registros} bytes={r.Bytes} esperado={r.BytesEsperados} " +
                              $"crc_gravado={r.CrcGravado:X8} crc_calculado={r.CrcCalculado:X8}");
            return ok ? 0 : 3;
        }

        // ------------------------------------------------------------------ DBF -> DBC
        private const int Win = 4096, WinMask = Win - 1, MaxLen = 518, HashBits = 16;
        private static readonly uint[] LenCode = new uint[520];
        private static readonly byte[] LenBits = new byte[520];
        private static readonly uint[] DistCode = new uint[64];
        private static readonly byte[] DistBits = new byte[64];

        static DbfDbc()
        {
            int[] baseLen = { 3, 2, 4, 5, 6, 7, 8, 9, 10, 12, 16, 24, 40, 72, 136, 264 };
            int[] extra = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            var (lc, ll) = Canonico(new byte[] { 2, 35, 36, 53, 38, 23 });
            var (dc, dl) = Canonico(new byte[] { 2, 20, 53, 230, 247, 151, 248 });
            for (int s = 0; s < 16; s++)
                for (int e = 0; e < (1 << extra[s]); e++)
                {
                    int L = baseLen[s] + e;
                    if (L > 519 || LenBits[L] != 0) continue;
                    LenCode[L] = 1u | (Inverter(lc[s], ll[s]) << 1) | ((uint)e << (1 + ll[s]));
                    LenBits[L] = (byte)(1 + ll[s] + extra[s]);
                }
            for (int s = 0; s < 64; s++) { DistCode[s] = Inverter(dc[s], dl[s]); DistBits[s] = (byte)dl[s]; }
        }

        private static (uint[] codes, int[] lens) Canonico(byte[] rep)
        {
            var lens = new System.Collections.Generic.List<int>();
            foreach (byte b in rep) for (int k = 0; k <= (b >> 4); k++) lens.Add(b & 15);
            int n = lens.Count;
            var ordem = new int[n];
            for (int i = 0; i < n; i++) ordem[i] = i;
            Array.Sort(ordem, (a, b) => lens[a] != lens[b] ? lens[a].CompareTo(lens[b]) : a.CompareTo(b));
            var codes = new uint[n];
            uint code = 0; int prev = 0;
            foreach (int s in ordem)
            {
                int L = lens[s];
                if (L == 0) continue;
                code <<= (L - prev); prev = L;
                codes[s] = code++;
            }
            return (codes, lens.ToArray());
        }

        private static uint Inverter(uint code, int L)
        {
            uint v = 0;
            for (int i = 0; i < L; i++) v |= (((code >> (L - 1 - i)) & 1u) ^ 1u) << i;
            return v;
        }

        private sealed class Bits
        {
            private readonly Stream _s;
            private readonly byte[] _o = new byte[1 << 20];
            private int _n;
            private ulong _buf;
            private int _cnt;
            public Bits(Stream s) { _s = s; }
            public void Put(uint v, int k)
            {
                _buf |= (ulong)v << _cnt; _cnt += k;
                while (_cnt >= 8)
                {
                    _o[_n++] = (byte)_buf; _buf >>= 8; _cnt -= 8;
                    if (_n == _o.Length) { _s.Write(_o, 0, _n); _n = 0; }
                }
            }
            public void Fim()
            {
                if (_cnt > 0) { _o[_n++] = (byte)_buf; _buf = 0; _cnt = 0; }
                if (_n > 0) { _s.Write(_o, 0, _n); _n = 0; }
            }
        }

        public static void ParaDbc(string dbfPath, string dbcPath)
        {
            string tmp = dbcPath + ".tmp_tabwon";
            try
            {
            using (var inp = new FileStream(dbfPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20))
            using (var outp = new FileStream(tmp, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20))
            {
                var h32 = new byte[32];
                if (inp.Read(h32, 0, 32) != 32) throw new InvalidDataException("arquivo DBF vazio ou truncado");
                int hl = BinaryPrimitives.ReadUInt16LittleEndian(h32.AsSpan(8));
                if (hl < 33 || hl > inp.Length) throw new InvalidDataException("cabeçalho DBF inválido");
                var hdr = new byte[hl];
                Array.Copy(h32, hdr, 32);
                inp.ReadExactly(hdr, 32, hl - 32);
                DbfHeader.Parse(hdr);
                outp.Write(hdr, 0, hl);
                outp.Write(new byte[4], 0, 4);
                uint crc = Crc32.Update(0, hdr);

                var bits = new Bits(outp);
                bits.Put(0, 8); bits.Put(6, 8);
                const int Bloco = 4 << 20;
                var buf = new byte[Win + Bloco + MaxLen + 8];
                var head = new long[1 << HashBits];
                Array.Fill(head, -1L);
                var prev = new long[Win];
                Array.Fill(prev, -1L);
                long inicio = 0;
                int fim = 0;
                bool eof = false;
                long i = 0;

                void Encher()
                {
                    long manter = Math.Max(inicio, i - Win);
                    int desl = (int)(manter - inicio);
                    if (desl > 0)
                    {
                        Buffer.BlockCopy(buf, desl, buf, 0, fim - desl);
                        fim -= desl; inicio = manter;
                    }
                    while (!eof && fim < buf.Length - 8)
                    {
                        int lidos = inp.Read(buf, fim, buf.Length - 8 - fim);
                        if (lidos <= 0) { eof = true; break; }
                        crc = Crc32.Update(crc, buf.AsSpan(fim, lidos));
                        fim += lidos;
                    }
                }

                Encher();
                fixed (long* hd = head, pv = prev)
                {
                    while (true)
                    {
                        if (!eof && i + MaxLen + 3 > inicio + fim) Encher();
                        long disp = inicio + fim - i;
                        if (disp <= 0) break;
                        int bi = (int)(i - inicio);
                        int melhor = 0; long dist = 0;
                        uint hsh = 0;
                        if (disp >= 3)
                        {
                            hsh = (((uint)buf[bi] << 16) | ((uint)buf[bi + 1] << 8) | buf[bi + 2]) * 2654435761u >> (32 - HashBits);
                            int lim = (int)Math.Min(MaxLen, disp);
                            long j = hd[hsh];
                            int tent = 48;
                            while (j >= 0 && i - j <= Win && tent-- > 0)
                            {
                                int bj = (int)(j - inicio);
                                if (bj >= 0 && buf[bj + melhor] == buf[bi + melhor] && buf[bj] == buf[bi] &&
                                    buf[bj + 1] == buf[bi + 1] && buf[bj + 2] == buf[bi + 2])
                                {
                                    int n = 3;
                                    while (n < lim && buf[bj + n] == buf[bi + n]) n++;
                                    if (n > melhor) { melhor = n; dist = i - j; if (n >= lim) break; }
                                }
                                long nj = pv[j & WinMask];
                                if (nj >= j) break;
                                j = nj;
                            }
                        }
                        int avancar;
                        if (melhor >= 3)
                        {
                            bits.Put(LenCode[melhor], LenBits[melhor]);
                            uint d = (uint)(dist - 1);
                            bits.Put(DistCode[d >> 6], DistBits[d >> 6]);
                            bits.Put(d & 63, 6);
                            avancar = melhor;
                        }
                        else
                        {
                            bits.Put((uint)buf[bi] << 1, 9);
                            avancar = 1;
                        }
                        for (int k = 0; k < avancar; k++, i++)
                        {
                            int bk = (int)(i - inicio);
                            if (i + 3 <= inicio + fim)
                            {
                                uint h = (((uint)buf[bk] << 16) | ((uint)buf[bk + 1] << 8) | buf[bk + 2]) * 2654435761u >> (32 - HashBits);
                                pv[i & WinMask] = hd[h];
                                hd[h] = i;
                            }
                        }
                    }
                }
                bits.Put(LenCode[519], LenBits[519]);
                bits.Fim();
                var c4 = new byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(c4, crc);
                outp.Seek(hl, SeekOrigin.Begin);
                outp.Write(c4, 0, 4);
            }
            File.Move(tmp, dbcPath, true);
            }
            catch { try { File.Delete(tmp); } catch { } throw; }
        }
    }
}

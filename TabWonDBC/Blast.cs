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

// Partes de terceiros neste arquivo:
// Baseado em blast.c (decodificador do formato PKWare DCL "implode"), parte do zlib/contrib:
//   Copyright (C) 2003, 2012, 2013 Mark Adler — licença zlib.
//   As tabelas de códigos (LitLen, LenLen, DistLen, bases e bits extras) e a lógica de
//   decodificação vêm desse arquivo. Esta versão foi ALTERADA: reescrita em C#, com
//   decodificação Huffman por tabela, buffer de bits de 64 bits e saída em fluxo.
//   Texto da licença zlib: https://zlib.net/zlib_license.html

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace TabWonDBC
{
    /// <summary>
    /// Descompactador PKWare DCL "implode" (o formato usado nos .dbc do DATASUS),
    /// equivalente ao blast.c do zlib, mas com decodificação Huffman por tabela
    /// (uma consulta por símbolo em vez de bit a bit) e buffer de bits de 64 bits.
    ///
    /// Funciona em fluxo: Decode() escreve em outBuf a partir de pos e para quando
    /// pos >= limit (pode passar do limite em até 518 + 8 bytes). Os 4096 bytes
    /// anteriores a pos precisam conter o histórico (janela) já descompactado.
    /// </summary>
    internal sealed unsafe class BlastDecoder
    {
        public const int Window = 4096;   // janela máxima (dict = 6)
        public const int MaxOverrun = 600; // 518 (maior cópia) + folga das cópias de 8 bytes

        private const int TableBits = 13; // MAXBITS do blast.c
        private const int TableMask = (1 << TableBits) - 1;

        // entrada = símbolo | (comprimento_em_bits << 16); 0 = código inválido
        private static readonly int[] LitTable;
        private static readonly int[] LenTable;
        private static readonly int[] DistTable;

        private static readonly byte[] LitLen = {
            11, 124, 8, 7, 28, 7, 188, 13, 76, 4, 10, 8, 12, 10, 12, 10, 8, 23, 8,
            9, 7, 6, 7, 8, 7, 6, 55, 8, 23, 24, 12, 11, 7, 9, 11, 12, 6, 7, 22, 5,
            7, 24, 6, 11, 9, 6, 7, 22, 7, 11, 38, 7, 9, 8, 25, 11, 8, 11, 9, 12,
            8, 12, 5, 38, 5, 38, 5, 11, 7, 5, 6, 21, 6, 10, 53, 8, 7, 24, 10, 27,
            44, 253, 253, 253, 252, 252, 252, 13, 12, 45, 12, 45, 12, 61, 12, 45,
            44, 173 };
        private static readonly byte[] LenLen = { 2, 35, 36, 53, 38, 23 };
        private static readonly byte[] DistLen = { 2, 20, 53, 230, 247, 151, 248 };

        // base + bits extras por símbolo de comprimento, empacotados: base | (extra << 16)
        private static readonly int[] LenBaseExtra;

        static BlastDecoder()
        {
            LitTable = BuildTable(LitLen);
            LenTable = BuildTable(LenLen);
            DistTable = BuildTable(DistLen);
            short[] bases = { 3, 2, 4, 5, 6, 7, 8, 9, 10, 12, 16, 24, 40, 72, 136, 264 };
            byte[] extra = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            LenBaseExtra = new int[16];
            for (int i = 0; i < 16; i++) LenBaseExtra[i] = (int)(ushort)bases[i] | (extra[i] << 16);
        }

        /// <summary>Monta a tabela simulando exatamente o decode() do blast.c para cada padrão de 13 bits.</summary>
        private static int[] BuildTable(byte[] rep)
        {
            var length = new int[256];
            int n = 0;
            foreach (byte b in rep)
            {
                int left = (b >> 4) + 1, len = b & 15;
                for (int k = 0; k < left; k++) length[n++] = len;
            }
            var count = new int[TableBits + 1];
            for (int s = 0; s < n; s++) count[length[s]]++;
            var offs = new int[TableBits + 2];
            for (int len = 1; len < TableBits; len++) offs[len + 1] = offs[len] + count[len];
            var symbol = new int[n];
            for (int s = 0; s < n; s++) if (length[s] != 0) symbol[offs[length[s]]++] = s;

            var table = new int[1 << TableBits];
            for (int v = 0; v < table.Length; v++)
            {
                int code = 0, first = 0, index = 0;
                for (int len = 1; len <= TableBits; len++)
                {
                    code |= ((v >> (len - 1)) & 1) ^ 1; // bits invertidos, como no blast.c
                    int c = count[len];
                    if (code < first + c)
                    {
                        table[v] = symbol[index + (code - first)] | (len << 16);
                        break;
                    }
                    index += c; first += c; first <<= 1; code <<= 1;
                }
            }
            return table;
        }

        private readonly byte* _in;
        private int _inPos;
        private readonly int _inEnd;
        private ulong _bitbuf;
        private int _bitcnt;
        private readonly bool _litCoded;
        private readonly int _dict;
        private long _totalOut;

        public bool Finished { get; private set; }
        public long TotalOut => _totalOut;

        /// <param name="input">ponteiro para os dados (que precisam ficar válidos durante o uso)</param>
        public BlastDecoder(byte* input, int offset, int length)
        {
            _in = input;
            _inPos = offset;
            _inEnd = offset + length;
            if (length < 2) throw new InvalidDataException("Arquivo DBC truncado (sem dados compactados).");
            int lit = _in[_inPos++];
            int dict = _in[_inPos++];
            if (lit > 1) throw new InvalidDataException("Invalid literal flag");
            if (dict < 4 || dict > 6) throw new InvalidDataException("Invalid dictionary size");
            _litCoded = lit == 1;
            _dict = dict;
        }

        /// <summary>
        /// Descompacta para o buffer (fixado na memória) a partir de pos até atingir limit
        /// ou o código de fim. Retorna a nova posição.
        /// </summary>
        public int Decode(byte* o, int pos, int limit)
        {
            if (Finished) return pos;
            int start = pos;
            long outBefore = _totalOut;
            byte* inp = _in;
            fixed (int* litT = LitTable, lenT = LenTable, distT = DistTable, lbe = LenBaseExtra)
            {
                ulong bitbuf = _bitbuf;
                int bitcnt = _bitcnt;
                int inPos = _inPos;
                int inEnd = _inEnd;
                int fastEnd = inEnd - 8;
                bool litCoded = _litCoded;
                int dict = _dict;
                ulong dictMask = (1UL << dict) - 1;

                while (pos < limit)
                {
                    // recarga: no meio do arquivo, sempre 8 bytes de uma vez (sem desvio)
                    if (inPos <= fastEnd)
                    {
                        bitbuf |= Unsafe.ReadUnaligned<ulong>(inp + inPos) << bitcnt;
                        inPos += (63 - bitcnt) >> 3;
                        bitcnt |= 56;
                    }
                    else
                    {
                        while (bitcnt <= 56 && inPos < inEnd)
                        {
                            bitbuf |= (ulong)inp[inPos++] << bitcnt;
                            bitcnt += 8;
                        }
                    }

                    if ((bitbuf & 1) == 0)
                    {
                        // literal
                        int sym;
                        if (!litCoded)
                        {
                            // literais sem código (o caso do DATASUS): até 5 seguidos sem recarregar
                            o[pos++] = (byte)(bitbuf >> 1);
                            bitbuf >>= 9;
                            bitcnt -= 9;
                            while (bitcnt >= 9 && (bitbuf & 1) == 0 && pos < limit)
                            {
                                o[pos++] = (byte)(bitbuf >> 1);
                                bitbuf >>= 9;
                                bitcnt -= 9;
                            }
                            if (bitcnt < 0) throw new InvalidDataException("Not enough input data");
                            continue;
                        }
                        else
                        {
                            int e = litT[(int)(bitbuf >> 1) & TableMask];
                            int l = e >> 16;
                            if (l == 0) throw new InvalidDataException("Invalid Huffman code");
                            bitbuf >>= 1 + l;
                            bitcnt -= 1 + l;
                            sym = e & 0xFFFF;
                        }
                        if (bitcnt < 0) throw new InvalidDataException("Not enough input data");
                        o[pos++] = (byte)sym;
                        continue;
                    }

                    // par comprimento/distância
                    int le = lenT[(int)(bitbuf >> 1) & TableMask];
                    int ll = le >> 16;
                    if (ll == 0) throw new InvalidDataException("Invalid Huffman code");
                    int be = lbe[le & 0xFFFF];
                    int ex = be >> 16;
                    bitbuf >>= 1 + ll;
                    int len = (be & 0xFFFF) + (int)(bitbuf & ((1UL << ex) - 1));
                    bitbuf >>= ex;
                    bitcnt -= 1 + ll + ex;
                    if (len == 519)
                    {
                        if (bitcnt < 0) throw new InvalidDataException("Not enough input data");
                        Finished = true;
                        break;
                    }
                    int de = distT[(int)bitbuf & TableMask];
                    int dl = de >> 16;
                    if (dl == 0) throw new InvalidDataException("Invalid Huffman code");
                    bitbuf >>= dl;
                    int dist;
                    if (len == 2)
                    {
                        dist = ((de & 0xFFFF) << 2) + (int)(bitbuf & 3) + 1;
                        bitbuf >>= 2;
                        bitcnt -= dl + 2;
                    }
                    else
                    {
                        dist = ((de & 0xFFFF) << dict) + (int)(bitbuf & dictMask) + 1;
                        bitbuf >>= dict;
                        bitcnt -= dl + dict;
                    }
                    if (bitcnt < 0) throw new InvalidDataException("Not enough input data");
                    if (dist > outBefore + (pos - start))
                        throw new InvalidDataException("Distance too far back");

                    byte* dst = o + pos;
                    byte* src = dst - dist;
                    pos += len;
                    if (dist >= 8)
                    {
                        // cópia em blocos de 8 bytes (pode escrever até 7 bytes além; há folga no buffer)
                        Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<ulong>(src));
                        if (len > 8)
                        {
                            byte* end = dst + len;
                            dst += 8; src += 8;
                            do
                            {
                                Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<ulong>(src));
                                dst += 8; src += 8;
                            } while (dst < end);
                        }
                    }
                    else if (dist == 1)
                    {
                        Unsafe.InitBlockUnaligned(dst, *src, (uint)len);
                    }
                    else
                    {
                        do { *dst++ = *src++; } while (--len != 0);
                    }
                }

                _bitbuf = bitbuf;
                _bitcnt = bitcnt;
                _inPos = inPos;
            }
            _totalOut = outBefore + (pos - start);
            return pos;
        }
    }
}

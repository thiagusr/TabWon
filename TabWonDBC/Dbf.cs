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
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace TabWonDBC
{
    internal enum ColKind { String, Int64, Double, DateTime, Bool }

    internal sealed class DbfField
    {
        public string Name;
        public char Type;
        public int Length;
        public int Decimals;
        public int Offset;   // deslocamento dentro do registro (o byte 0 é o flag de apagado)
        public ColKind Kind;
    }

    internal sealed class DbfHeader
    {
        public uint RecordCount;
        public int HeaderLength;
        public int RecordLength;
        public List<DbfField> Fields = new();

        public static DbfHeader Parse(ReadOnlySpan<byte> b)
        {
            if (b.Length < 32) throw new InvalidDataException("Input stream too short");
            var h = new DbfHeader
            {
                RecordCount = BinaryPrimitives.ReadUInt32LittleEndian(b.Slice(4)),
                HeaderLength = BinaryPrimitives.ReadUInt16LittleEndian(b.Slice(8)),
                RecordLength = BinaryPrimitives.ReadUInt16LittleEndian(b.Slice(10)),
            };
            if (h.HeaderLength < 33 || h.HeaderLength > b.Length)
                throw new InvalidDataException("Invalid header size found in .dbc file");

            int offset = 1;
            for (int p = 32; p + 32 <= h.HeaderLength && b[p] != 0x0D; p += 32)
            {
                int nameLen = 0;
                while (nameLen < 11 && b[p + nameLen] != 0) nameLen++;
                var f = new DbfField
                {
                    Name = Encoding.Latin1.GetString(b.Slice(p, nameLen)).Trim(),
                    Type = (char)b[p + 11],
                    Length = b[p + 16],
                    Decimals = b[p + 17],
                    Offset = offset,
                };
                // Mesmo mapeamento de tipos do DbfDataReader original (DbcToDbf):
                // N/F sem decimais -> Int64, N/F com decimais -> Decimal (gravado como double),
                // D -> DateTime, L -> Boolean, qualquer outro -> String.
                f.Kind = char.ToUpperInvariant(f.Type) switch
                {
                    'N' or 'F' => f.Decimals > 0 ? ColKind.Double : ColKind.Int64,
                    'D' => ColKind.DateTime,
                    'L' => ColKind.Bool,
                    _ => ColKind.String,
                };
                offset += f.Length;
                h.Fields.Add(f);
            }
            if (h.Fields.Count == 0) throw new InvalidDataException("DBF sem campos.");
            if (offset > h.RecordLength)
                throw new InvalidDataException($"Tamanho de registro inconsistente ({offset} > {h.RecordLength}).");
            return h;
        }
    }

    /// <summary>Conversões de célula, reproduzindo o comportamento do DbfDataReader original.</summary>
    internal static unsafe class Cell
    {
        // char.IsWhiteSpace para cada byte Latin-1 (= o que string.Trim() remove)
        public static readonly bool[] Ws = BuildWs();
        private static bool[] BuildWs()
        {
            var t = new bool[256];
            for (int i = 0; i < 256; i++) t[i] = char.IsWhiteSpace((char)i);
            return t;
        }

        /// <summary>Equivalente a string.Trim() sobre os bytes Latin-1: devolve o início e o tamanho.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Trim(byte* p, int w, out int start)
        {
            int s = 0, e = w;
            while (s < e && Ws[p[s]]) s++;
            while (e > s && Ws[p[e - 1]]) e--;
            start = s;
            return e - s;
        }

        /// <summary>Texto Latin-1 com Trim(); vazio quando só há espaços (o original gravava "").</summary>
        public static string MakeString(byte* p, int w)
        {
            int s = 0, e = w;
            while (s < e && Ws[p[s]]) s++;
            while (e > s && Ws[p[e - 1]]) e--;
            if (e == s) return "";
            return Encoding.Latin1.GetString(p + s, e - s);
        }

        private static string TrimmedOrNull(byte* p, int w)
        {
            string s = MakeString(p, w);
            return s.Length == 0 ? null : s;
        }

        public static long ParseInt64(byte* p, int w)
        {
            int i = 0;
            while (i + 8 <= w && Unsafe.ReadUnaligned<ulong>(p + i) == 0x2020202020202020UL) i += 8;
            while (i < w && p[i] == (byte)' ') i++;
            if (i == w) return 0; // nulo -> 0
            bool neg = false;
            if (p[i] == (byte)'-') { neg = true; i++; }
            else if (p[i] == (byte)'+') i++;
            int ds = i;
            ulong v = 0;
            while (i < w)
            {
                uint d = (uint)(p[i] - (byte)'0');
                if (d > 9) break;
                v = v * 10 + d;
                i++;
            }
            int nd = i - ds;
            while (i < w && p[i] == (byte)' ') i++;
            if (nd > 0 && nd <= 18 && i == w)
                return neg ? -(long)v : (long)v;
            return ParseInt64Slow(p, w);
        }

        private static long ParseInt64Slow(byte* p, int w)
        {
            string s = TrimmedOrNull(p, w);
            if (s == null) return 0;
            return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out long r) ? r : 0;
        }

        private static readonly double[] Pow10 = {
            1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11,
            1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20, 1e21, 1e22 };

        public static double ParseDouble(byte* p, int w)
        {
            int i = 0;
            while (i + 8 <= w && Unsafe.ReadUnaligned<ulong>(p + i) == 0x2020202020202020UL) i += 8;
            while (i < w && p[i] == (byte)' ') i++;
            if (i == w) return 0.0; // nulo -> 0
            bool neg = false;
            if (p[i] == (byte)'-') { neg = true; i++; }
            else if (p[i] == (byte)'+') i++;
            ulong m = 0;
            int nd = 0, scale = 0;
            while (i < w)
            {
                uint d = (uint)(p[i] - (byte)'0');
                if (d > 9) break;
                m = m * 10 + d; nd++; i++;
            }
            if (i < w && p[i] == (byte)'.')
            {
                i++;
                while (i < w)
                {
                    uint d = (uint)(p[i] - (byte)'0');
                    if (d > 9) break;
                    m = m * 10 + d; nd++; scale++; i++;
                }
            }
            while (i < w && p[i] == (byte)' ') i++;
            if (nd > 0 && i == w && m < (1UL << 53) && nd <= 19 && scale <= 22 && !(neg && m == 0))
            {
                // Mesmo cálculo que decimal -> double faz no .NET (mantissa / 10^escala),
                // exato aqui porque a mantissa e a potência de 10 cabem sem arredondar num double.
                double r = (double)(long)m / Pow10[scale];
                return neg ? -r : r;
            }
            if (nd > 0 && nd <= 19 && i == w && !(neg && m == 0))
            {
                // Mesma representação que decimal.Parse produziria (mantissa + escala),
                // depois a mesma conversão decimal -> double do Convert.ToDouble original.
                var dec = new decimal((int)(uint)m, (int)(uint)(m >> 32), 0, neg, (byte)scale);
                return (double)dec;
            }
            return ParseDoubleSlow(p, w);
        }

        private static double ParseDoubleSlow(byte* p, int w)
        {
            string s = TrimmedOrNull(p, w);
            if (s == null) return 0.0;
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d) ? (double)d : 0.0;
        }

        public static DateTime ParseDate(byte* p, int w)
        {
            int s = 0, e = w;
            while (s < e && p[s] == (byte)' ') s++;
            while (e > s && p[e - 1] == (byte)' ') e--;
            if (s == e) return default;
            if (e - s == 8)
            {
                byte* q = p + s;
                bool digits = true;
                for (int k = 0; k < 8; k++) if ((uint)(q[k] - (byte)'0') > 9) { digits = false; break; }
                if (digits)
                {
                    int y = (q[0] - '0') * 1000 + (q[1] - '0') * 100 + (q[2] - '0') * 10 + (q[3] - '0');
                    int mo = (q[4] - '0') * 10 + (q[5] - '0');
                    int d = (q[6] - '0') * 10 + (q[7] - '0');
                    if (y >= 1 && mo >= 1 && mo <= 12 && d >= 1 && d <= DateTime.DaysInMonth(y, mo))
                        return new DateTime(y, mo, d);
                    return default;
                }
            }
            string str = TrimmedOrNull(p, w);
            if (str == null) return default;
            return DateTime.TryParseExact(str, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? dt : default;
        }

        public static bool ParseBool(byte* p, int w)
        {
            int s = 0;
            while (s < w && Ws[p[s]]) s++;
            if (s == w) return false; // nulo -> false
            byte c = p[s];
            return c == (byte)'T' || c == (byte)'t' || c == (byte)'Y' || c == (byte)'y';
        }
    }
}

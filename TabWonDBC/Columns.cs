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
using System.Runtime.CompilerServices;

namespace TabWonDBC
{
    /// <summary>Dados de uma coluna dentro de um row group.</summary>
    internal abstract class ColumnData
    {
        public abstract void Prepare(int capacity);
    }

    internal sealed class Int64Column : ColumnData
    {
        public long[] Values;
        public override void Prepare(int capacity) => Values = GC.AllocateUninitializedArray<long>(capacity);
    }

    internal sealed class DoubleColumn : ColumnData
    {
        public double[] Values;
        public override void Prepare(int capacity) => Values = GC.AllocateUninitializedArray<double>(capacity);
    }

    internal sealed class DateColumn : ColumnData
    {
        public DateTime[] Values;
        public override void Prepare(int capacity) => Values = GC.AllocateUninitializedArray<DateTime>(capacity);
    }

    internal sealed class BoolColumn : ColumnData
    {
        public bool[] Values;
        public override void Prepare(int capacity) => Values = GC.AllocateUninitializedArray<bool>(capacity);
    }

    /// <summary>
    /// Texto com dicionário: cada linha guarda só o índice do valor; o dicionário (valores
    /// distintos, já sem espaços nas pontas) é montado durante a leitura e vira a página de
    /// dicionário do Parquet.
    /// </summary>
    internal sealed unsafe class DictStringColumn : ColumnData
    {
        public int[] Ids;
        public byte[] Keys = new byte[4096];   // valores distintos (Latin-1, já com Trim) concatenados
        public int[] KeyStart = new int[257];  // KeyStart[i]..KeyStart[i+1]
        public int Count;
        private int _keysLen;
        private int[] _table = new int[512];   // índice + 1; 0 = vazio
        private int _mask = 511;

        public override void Prepare(int capacity) => Ids = GC.AllocateUninitializedArray<int>(capacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Hash(byte* p, int n)
        {
            ulong h = 0x9E3779B97F4A7C15UL ^ (ulong)n;
            int i = 0;
            for (; i + 8 <= n; i += 8)
                h = (h ^ Unsafe.ReadUnaligned<ulong>(p + i)) * 0xFF51AFD7ED558CCDUL;
            if (i < n)
            {
                ulong t = 0;
                if (n >= 8) t = Unsafe.ReadUnaligned<ulong>(p + n - 8);
                else for (int k = 0; k < n; k++) t |= (ulong)p[k] << (8 * k);
                h = (h ^ t) * 0xC4CEB9FE1A85EC53UL;
            }
            h ^= h >> 33;
            h *= 0xFF51AFD7ED558CCDUL;
            h ^= h >> 29;
            return (uint)h;
        }

        // ---- cache pelos bytes crus do campo (campos de até 16 bytes) ----
        // Mapeia o conteúdo bruto (sem Trim) direto para o índice do dicionário: no caso comum é
        // uma multiplicação, uma comparação de 64 bits e pronto. Valores brutos diferentes que
        // ficam iguais após o Trim apontam para a mesma entrada do dicionário.
        private ulong[] _rk0, _rk1;
        private int[] _rid;      // índice + 1; 0 = vazio
        private int _rCount, _rShift;

        private void InitRaw()
        {
            int cap = 1024;
            _rk0 = new ulong[cap]; _rk1 = new ulong[cap]; _rid = new int[cap];
            _rShift = 64 - 10;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIdRaw(byte* p, int w, ulong mask0, ulong mask1)
        {
            ulong k0 = Unsafe.ReadUnaligned<ulong>(p) & mask0;
            ulong k1 = mask1 == 0 ? 0 : Unsafe.ReadUnaligned<ulong>(p + 8) & mask1;
            if (_rid == null) InitRaw();
            int m = _rid.Length - 1;
            int slot = (int)(((k0 ^ (k1 * 0xC2B2AE3D27D4EB4FUL)) * 0x9E3779B97F4A7C15UL) >> _rShift);
            while (true)
            {
                int id = _rid[slot];
                if (id == 0) break;
                if (_rk0[slot] == k0 && _rk1[slot] == k1) return id - 1;
                slot = (slot + 1) & m;
            }
            return AddRaw(p, w, k0, k1, slot);
        }

        private int AddRaw(byte* p, int w, ulong k0, ulong k1, int slot)
        {
            int len = Cell.Trim(p, w, out int s);
            int id = GetId(p + s, len);
            if ((_rCount + 1) * 2 > _rid.Length)
            {
                var o0 = _rk0; var o1 = _rk1; var oi = _rid;
                int cap = oi.Length * 2;
                _rk0 = new ulong[cap]; _rk1 = new ulong[cap]; _rid = new int[cap];
                _rShift--;
                for (int i = 0; i < oi.Length; i++) if (oi[i] != 0) Put(o0[i], o1[i], oi[i]);
                Put(k0, k1, id + 1);
            }
            else
            {
                _rk0[slot] = k0; _rk1[slot] = k1; _rid[slot] = id + 1;
            }
            _rCount++;
            return id;
        }

        private void Put(ulong k0, ulong k1, int idPlus1)
        {
            int m = _rid.Length - 1;
            int slot = (int)(((k0 ^ (k1 * 0xC2B2AE3D27D4EB4FUL)) * 0x9E3779B97F4A7C15UL) >> _rShift);
            while (_rid[slot] != 0) slot = (slot + 1) & m;
            _rk0[slot] = k0; _rk1[slot] = k1; _rid[slot] = idPlus1;
        }

        /// <summary>Índice do valor (bytes já sem espaços nas pontas) no dicionário.</summary>
        public int GetId(byte* p, int n)
        {
            uint h = Hash(p, n);
            int slot = (int)(h & (uint)_mask);
            fixed (byte* keys = Keys)
            {
                while (true)
                {
                    int idx = _table[slot];
                    if (idx == 0) break;
                    idx--;
                    int s = KeyStart[idx];
                    if (KeyStart[idx + 1] - s == n && new ReadOnlySpan<byte>(p, n).SequenceEqual(new ReadOnlySpan<byte>(keys + s, n)))
                        return idx;
                    slot = (slot + 1) & _mask;
                }
            }
            return Add(p, n, slot);
        }

        private int Add(byte* p, int n, int slot)
        {
            if ((Count + 1) * 2 > _table.Length)
            {
                Rehash(_table.Length * 2);
                slot = (int)(Hash(p, n) & (uint)_mask);
                while (_table[slot] != 0) slot = (slot + 1) & _mask;
            }
            if (Count + 2 > KeyStart.Length) Array.Resize(ref KeyStart, KeyStart.Length * 2);
            if (_keysLen + n > Keys.Length) Array.Resize(ref Keys, Math.Max(Keys.Length * 2, _keysLen + n));
            new ReadOnlySpan<byte>(p, n).CopyTo(Keys.AsSpan(_keysLen));
            KeyStart[Count] = _keysLen;
            _keysLen += n;
            KeyStart[Count + 1] = _keysLen;
            _table[slot] = Count + 1;
            return Count++;
        }

        private void Rehash(int size)
        {
            _table = new int[size];
            _mask = size - 1;
            fixed (byte* keys = Keys)
            {
                for (int i = 0; i < Count; i++)
                {
                    int s = KeyStart[i];
                    int slot = (int)(Hash(keys + s, KeyStart[i + 1] - s) & (uint)_mask);
                    while (_table[slot] != 0) slot = (slot + 1) & _mask;
                    _table[slot] = i + 1;
                }
            }
        }
    }

    /// <summary>Texto sem dicionário (colunas com muitos valores distintos): bytes UTF-8 concatenados.</summary>
    internal sealed class PlainStringColumn : ColumnData
    {
        public byte[] Data;
        public int DataLen;
        public int[] Ends;  // Ends[i] = fim do valor i em Data
        private readonly int _width;

        public PlainStringColumn(int width) { _width = width; }

        public override void Prepare(int capacity)
        {
            Ends = GC.AllocateUninitializedArray<int>(capacity);
            Data = GC.AllocateUninitializedArray<byte>((int)Math.Min((long)capacity * Math.Max(1, _width) + 16, Array.MaxLength));
            DataLen = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Append(int row, byte* p, int n)
        {
            if (DataLen + 2 * n > Data.Length)
                Array.Resize(ref Data, (int)Math.Min(Math.Max((long)Data.Length * 2, DataLen + 2L * n), Array.MaxLength));
            fixed (byte* d = Data) DataLen += Latin1.ToUtf8(p, n, d + DataLen);
            Ends[row] = DataLen;
        }
    }

    internal static unsafe class Latin1
    {
        /// <summary>Latin-1 -> UTF-8 (o mesmo resultado de Encoding.UTF8.GetBytes(Encoding.Latin1.GetString(...))).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToUtf8(byte* s, int n, byte* d)
        {
            byte* d0 = d;
            int i = 0;
            // caminho rápido: 8 bytes ASCII de cada vez
            for (; i + 8 <= n; i += 8)
            {
                ulong v = Unsafe.ReadUnaligned<ulong>(s + i);
                if ((v & 0x8080808080808080UL) != 0) break;
                Unsafe.WriteUnaligned(d, v);
                d += 8;
            }
            for (; i < n; i++)
            {
                byte b = s[i];
                if (b < 0x80) *d++ = b;
                else { *d++ = (byte)(0xC0 | (b >> 6)); *d++ = (byte)(0x80 | (b & 0x3F)); }
            }
            return (int)(d - d0);
        }

        public static int Utf8Length(byte* s, int n)
        {
            int extra = 0;
            for (int i = 0; i < n; i++) extra += s[i] >> 7;
            return n + extra;
        }
    }
}

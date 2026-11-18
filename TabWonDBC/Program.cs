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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TabWonDBC
{
    class Program
    {
        const string Aviso =
@"TabWonDBC 2.1 — conversor DBC (DATASUS) -> Parquet, DBC <-> DBF
Programa original de Thiago Rodrigues (thiagusr). Copyright (C) 2026 Thiago Rodrigues.
Software livre sob a licença GNU GPL versão 3 (arquivo LICENSE), SEM NENHUMA GARANTIA.
";

        const string Uso = Aviso +
@"
Uso:
  TabWonDBC <entrada.dbc> <saida.parquet> [opções]
  TabWonDBC --lote <pasta_saida> <arquivo.dbc | pasta | curinga>... [opções]
  TabWonDBC --para-dbf <entrada.dbc> <saida.dbf>   descompacta o DBC em DBF (confere o CRC)
  TabWonDBC --para-dbc <entrada.dbf> <saida.dbc>   compacta o DBF no formato DBC do DATASUS

Opções:
  --rowgroup N   linhas por row group do Parquet (padrão 1000000; 0 = um só, como a versão antiga)
  --threads N    threads de conversão por arquivo (padrão: núcleos da máquina)
  --jobs N       no modo --lote, quantos arquivos converter ao mesmo tempo (padrão: núcleos/2)
  --tempo        mostra no stderr o tempo de cada etapa
  --versao       mostra a versão, o autor e a licença";

        static int Main(string[] args)
        {
            if (args.Length >= 1 && (args[0].Equals("--para-dbf", StringComparison.OrdinalIgnoreCase) ||
                                     args[0].Equals("--para-dbc", StringComparison.OrdinalIgnoreCase)))
            {
                if (args.Length < 3)
                {
                    Console.Error.WriteLine("Uso: TabWonDBC --para-dbf <entrada.dbc> <saida.dbf>  |  --para-dbc <entrada.dbf> <saida.dbc>");
                    return 1;
                }
                try
                {
                    if (args[0].Equals("--para-dbf", StringComparison.OrdinalIgnoreCase)) DbfDbc.ParaDbf(args[1], args[2]);
                    else DbfDbc.ParaDbc(args[1], args[2]);
                    Console.WriteLine($"OK: {args[2]}");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"ERRO: {ex.Message}");
                    return 2;
                }
            }
            var opt = new ConvertOptions();
            string loteDir = null;
            int jobs = Math.Max(1, Environment.ProcessorCount / 2);
            var pos = new List<string>();

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string a = args[i];
                    switch (a.ToLowerInvariant())
                    {
                        case "--rowgroup": opt.RowGroupSize = int.Parse(args[++i]); break;
                        case "--threads": opt.Threads = Math.Max(1, int.Parse(args[++i])); break;
                        case "--jobs": jobs = Math.Max(1, int.Parse(args[++i])); break;
                        case "--tempo": opt.Timing = true; break;
                        case "--lote": loteDir = args[++i]; break;
                        case "-h": case "--help": case "/?": Console.WriteLine(Uso); return 0;
                        case "--versao": case "--version": case "--licenca": Console.WriteLine(Aviso); return 0;
                        default: pos.Add(a); break;
                    }
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine(Uso);
                return 1;
            }

            if (loteDir == null)
            {
                // modo compatível com a versão anterior
                if (pos.Count < 2)
                {
                    Console.Error.WriteLine("Uso: TabWonDBC <entrada.dbc> <saida.parquet>");
                    return 1;
                }
                try
                {
                    Converter.Convert(pos[0], pos[1], opt);
                    Console.WriteLine($"OK: {pos[1]}");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"ERRO: {ex.Message}");
                    return 2;
                }
            }

            // modo lote: vários arquivos numa única execução, alguns em paralelo
            var inputs = ExpandInputs(pos);
            if (inputs.Count == 0)
            {
                Console.Error.WriteLine("ERRO: nenhum arquivo .dbc encontrado.");
                return 1;
            }
            Directory.CreateDirectory(loteDir);
            jobs = Math.Min(jobs, inputs.Count);
            opt.Threads = Math.Max(1, Math.Min(opt.Threads, Environment.ProcessorCount / jobs + 1));
            int failures = 0;
            var outLock = new object();
            Parallel.ForEach(inputs, new ParallelOptions { MaxDegreeOfParallelism = jobs }, input =>
            {
                string output = Path.Combine(loteDir, Path.GetFileNameWithoutExtension(input) + ".parquet");
                try
                {
                    Converter.Convert(input, output, opt);
                    lock (outLock) Console.WriteLine($"OK: {output}");
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failures);
                    lock (outLock) Console.Error.WriteLine($"ERRO: {input}: {ex.Message}");
                }
            });
            return failures == 0 ? 0 : 2;
        }

        static List<string> ExpandInputs(List<string> items)
        {
            var list = new List<string>();
            foreach (var it in items)
            {
                if (Directory.Exists(it))
                {
                    list.AddRange(Directory.GetFiles(it, "*.dbc"));
                }
                else if (it.IndexOfAny(new[] { '*', '?' }) >= 0)
                {
                    string dir = Path.GetDirectoryName(it);
                    if (string.IsNullOrEmpty(dir)) dir = ".";
                    if (Directory.Exists(dir)) list.AddRange(Directory.GetFiles(dir, Path.GetFileName(it)));
                }
                else
                {
                    list.Add(it);
                }
            }
            return list.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}

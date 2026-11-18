# TabWonDBC

**Conversor de arquivos `.dbc` para Apache Parquet — módulo integrante do TabWon**

**Autor original:** Thiago Rodrigues (thiagusr)  
**Licença:** [GNU General Public License v3.0 (GPL-3.0-only)](LICENSE), com [termo adicional de atribuição de autoria](AVISO.md).

O **TabWonDBC** é um programa de linha de comando que converte arquivos `.dbc`, utilizados nas tabelas compatíveis com o **TabWin**, para o formato **Apache Parquet**. O TabWin é mantido pelo **DATASUS**. O TabWonDBC integra o projeto **TabWon**, mas também pode ser compilado e executado de forma independente.

O programa lê o conteúdo compactado do `.dbc` e gera diretamente um arquivo `.parquet`, **sem criar um `.dbf` temporário**. O resultado pode ser utilizado por ferramentas como Python (pandas, Polars e PyArrow), R, DuckDB, Power BI e Apache Spark.

> **Para quem é este README?** As primeiras seções explicam como baixar o projeto do GitHub, compilar o executável e fazer uma conversão, mesmo sem conhecimento de C#. As seções seguintes documentam a arquitetura, as opções de execução, os formatos de dados, o desempenho e os detalhes de implementação.

### Repositório oficial

O **TabWonDBC** é um módulo integrante do projeto **TabWon**, disponibilizado como software livre e de código aberto.

O código-fonte, a documentação e as atualizações do projeto estão disponíveis no GitHub:

**[GitHub — thiagusr/tabwon](https://github.com/thiagusr/tabwon)**

O repositório permite consultar o código-fonte, acompanhar o desenvolvimento, obter as versões mais recentes e contribuir para o aprimoramento do projeto.

## Sumário

- [1. Guia rápido: compilar e usar no Windows](#1-guia-rápido-compilar-e-usar-no-windows)
- [2. O que o TabWonDBC faz](#2-o-que-o-tabwondbc-faz)
- [3. Requisitos e detalhes técnicos](#3-requisitos-e-detalhes-técnicos)
- [4. Compilação e publicação: referência técnica](#4-compilação-e-publicação-referência-técnica)
- [5. Como usar o executável](#5-como-usar-o-executável)
- [6. Funcionamento interno](#6-funcionamento-interno)
- [7. Estrutura do projeto e funções](#7-estrutura-do-projeto-e-funções)
- [8. Formato de saída: Apache Parquet](#8-formato-de-saída-apache-parquet)
- [9. Desempenho](#9-desempenho)
- [10. Limitações conhecidas](#10-limitações-conhecidas)
- [11. Perguntas frequentes](#11-perguntas-frequentes)
- [12. Créditos, licença e avisos](#12-créditos-licença-e-avisos)

---

## 1. Guia rápido: compilar e usar no Windows

Este procedimento usa o **Prompt de Comando do Windows (cmd)**. Você não precisa abrir o Visual Studio nem conhecer a linguagem C#: basta executar os comandos na ordem indicada.

### Antes de começar

Tenha em mãos:

- Um computador com **Windows de 64 bits** (o procedimento foi preparado para Windows 10 ou 11).
- O **.NET 8 SDK** instalado no computador em que a compilação será realizada.
- Uma conexão com a internet para a primeira restauração das dependências.
- O **código-fonte completo do TabWonDBC**, obtido no repositório do projeto no **GitHub**.

**SDK e Runtime não são a mesma coisa:** o SDK contém as ferramentas de compilação. O Runtime serve para executar aplicações que dependem de uma instalação do .NET. A publicação autocontida prevista para este projeto incorpora o ambiente necessário ao executável, dispensando a instalação do Runtime no computador de destino.

### Passo 1 — Conferir se o .NET 8 SDK está instalado

Abra o menu **Iniciar**, digite `cmd` e pressione **Enter**. Na janela do Prompt de Comando, execute:

```bat
dotnet --version
```

Se o SDK 8 estiver selecionado, será mostrado um número como `8.0.xxx` (os últimos dígitos variam conforme a atualização instalada).

**Atenção:** se aparecer `9.0.xxx`, `10.0.xxx` ou outra versão, isso **não significa necessariamente** que o .NET 8 esteja ausente. Para conferir todos os SDKs instalados, execute também:

```bat
dotnet --list-sdks
```

Procure uma linha iniciada por `8.0.`. Se o comando `dotnet` não for reconhecido ou se for necessário instalar o SDK 8, acesse o site oficial da Microsoft:

**[Baixar o .NET 8 SDK para Windows x64](https://dotnet.microsoft.com/download/dotnet/8.0)**

Selecione **SDK** (não apenas *Runtime*). Após instalar, feche a janela do Prompt e abra outra para refazer a verificação.

> **Observação sobre manutenção:** o projeto recebido tem como alvo o .NET 8. O suporte oficial dessa versão terminou em **10 de novembro de 2026**. Os passos abaixo mantêm a compatibilidade com o projeto atual; para manutenção futura, recomenda-se avaliar uma migração para uma versão do .NET com suporte ativo. Consulte a [política de suporte da Microsoft](https://dotnet.microsoft.com/platform/support/policy/dotnet-core).

### Passo 2 — Criar a pasta de trabalho

Ainda no Prompt de Comando, execute:

```bat
mkdir C:\TabWonDBC
cd /d C:\TabWonDBC
```

O primeiro comando cria a pasta `C:\TabWonDBC`. O segundo abre essa pasta no Prompt, mesmo que o terminal esteja em outra unidade de disco.

Se aparecer um aviso de que a pasta já existe, você poderá continuar normalmente.

### Passo 3 — Baixar e extrair o ZIP do GitHub

1. Abra o **repositório do TabWonDBC no GitHub** (ou a pasta correspondente dentro do repositório do TabWon, conforme a organização adotada pelo projeto).
2. Para baixar pelo navegador, clique em **Code → Download ZIP**, quando essa opção estiver disponível. Se você já recebeu o ZIP completo do código-fonte, utilize esse arquivo.
3. Extraia os arquivos para `C:\TabWonDBC`.
4. Confira se `TabWonDBC.csproj` ficou **diretamente** dentro de `C:\TabWonDBC`.

**Importante:** não deixe os arquivos em uma subpasta adicional criada durante a extração (por exemplo, `C:\TabWonDBC\TabWonDBC-main\`). Se isso acontecer, mova os arquivos do projeto para `C:\TabWonDBC` antes de continuar.

### Passo 4 — Conferir os 10 arquivos do projeto

Digite:

```bat
dir /b
```

Antes de compilar, a distribuição descrita nesta documentação deve apresentar estes **10 arquivos** na pasta principal:

```text
AVISO.md
Blast.cs
Columns.cs
Converter.cs
Dbf.cs
LICENSE
Parquet.cs
Program.cs
README.md
TabWonDBC.csproj
```

A ordem mostrada pelo Windows pode ser diferente. Se `TabWonDBC.csproj` não aparecer, verifique se você está na pasta correta ou se o ZIP foi extraído com uma pasta intermediária. Se a versão disponibilizada no GitHub tiver uma estrutura diferente, siga os arquivos efetivamente presentes no repositório.

### Passo 5 — Baixar as dependências e compilar

Com o Prompt ainda em `C:\TabWonDBC`, execute um comando de cada vez:

```bat
dotnet restore
```

```bat
dotnet publish -c Release
```

- `dotnet restore` baixa ou recupera do cache as dependências do projeto, incluindo o pacote **Snappier**.
- `dotnet publish -c Release` compila o código em modo **Release** e gera os arquivos para distribuição, conforme a configuração de `TabWonDBC.csproj`.

Na primeira execução, o processo pode baixar também componentes necessários à publicação para Windows. A duração depende da conexão e do computador.

Se tudo ocorrer bem, a saída do terminal indicará que a compilação/publicação foi concluída com sucesso, possivelmente com a mensagem `Build succeeded` ou `Compilação com êxito`.

**Pasta esperada de publicação:**

```text
C:\TabWonDBC\bin\Release\net8.0\win-x64\publish\
```

> Se a pasta não for criada nesse caminho, confira primeiro as configurações de publicação do arquivo `TabWonDBC.csproj` e a mensagem final do comando `dotnet publish`.

### Passo 6 — Copiar documentação e licença, quando necessário

**Se você pretende distribuir ou utilizar o TabWonDBC de forma independente**, copie o README, a licença e o aviso para a mesma pasta do executável. Execute os comandos a partir de `C:\TabWonDBC`:

```bat
copy README.md bin\Release\net8.0\win-x64\publish\
copy LICENSE bin\Release\net8.0\win-x64\publish\
copy AVISO.md bin\Release\net8.0\win-x64\publish\
```

**Se o executável for utilizado como parte integrante do TabWon**, mantenha a documentação e os avisos de licença correspondentes no projeto principal, acompanhando a distribuição. Nesse caso, não é necessário manter uma segunda cópia de cada documento ao lado do módulo, desde que as obrigações de licença e disponibilização do código-fonte sejam cumpridas.

### Passo 7 — Testar se o executável funciona

Entre na pasta de publicação:

```bat
cd /d C:\TabWonDBC\bin\Release\net8.0\win-x64\publish
```

Em seguida, execute:

```bat
TabWonDBC.exe --versao
```

O programa deverá apresentar as informações de versão, autoria e licença previstas na aplicação. Esse teste verifica se o executável inicia corretamente; não é, por si só, um teste de conversão de dados.

### Passo 8 — Converter um arquivo `.dbc`

Para testar uma conversão real, coloque `seu_arquivo.dbc` na pasta de publicação ou informe o caminho completo até ele. Estando na pasta em que se encontra `TabWonDBC.exe`, execute:

```bat
TabWonDBC.exe seu_arquivo.dbc seu_arquivo.parquet --tempo
```

Nesse exemplo:

- `seu_arquivo.dbc` é o **arquivo de entrada** que será convertido.
- `seu_arquivo.parquet` é o **arquivo de saída** que será criado.
- `--tempo` solicita a exibição do tempo gasto nas etapas da conversão.

Se o arquivo estiver em outra pasta, informe os caminhos entre aspas, especialmente quando contiverem espaços:

```bat
TabWonDBC.exe "C:\dados\seu_arquivo.dbc" "C:\dados\seu_arquivo.parquet" --tempo
```

Em caso de sucesso, o programa informa o nome do arquivo Parquet gerado. Verifique também se esse arquivo foi criado na pasta de destino.

### Passo 9 — Distribuir o módulo ou atualizar o TabWon

A pasta `publish` é a pasta de **distribuição** do aplicativo. Na configuração de publicação em arquivo único prevista pelo projeto, o elemento principal é `TabWonDBC.exe`; em uma distribuição independente, mantenha também `README.md`, `LICENSE` e `AVISO.md`, além de qualquer outro arquivo que a publicação efetivamente exigir.

**Para atualizar o TabWon:**

1. Feche o TabWon, caso esteja aberto.
2. Localize a pasta **`TabWon_configs`** no diretório da instalação do projeto principal.
3. Faça uma cópia de segurança do `TabWonDBC.exe` antigo.
4. Copie o novo `TabWonDBC.exe`, gerado na pasta `publish`, para `TabWon_configs`, substituindo o executável anterior.
5. Preserve o restante da estrutura e os arquivos de configuração do TabWon.
6. Abra o TabWon e valide uma conversão a partir do aplicativo principal.

O TabWon utiliza um **caminho fixo** para chamar esse módulo. Portanto, apenas compilar o TabWonDBC em `C:\TabWonDBC` não atualiza automaticamente o executável utilizado pelo TabWon.

### Problemas mais comuns

| Mensagem ou situação | O que verificar |
|---|---|
| `dotnet` não é reconhecido | O SDK não está instalado ou o Prompt estava aberto antes da instalação. Instale o SDK e abra um novo Prompt. |
| `dotnet --version` exibe uma versão diferente de 8 | Execute `dotnet --list-sdks`. A versão ativa não é necessariamente a única instalada. |
| `NETSDK1045` | O SDK usado não oferece suporte ao *framework* solicitado. Verifique a instalação do SDK e a seleção da versão. |
| `NU1101` ou erro ao localizar o Snappier | Confira a internet, o acesso ao NuGet e, se aplicável, o proxy corporativo. |
| `MSB1009: Project file does not exist` | Você provavelmente está fora da pasta que contém `TabWonDBC.csproj`. Use `cd /d C:\TabWonDBC`. |
| A pasta `publish` não foi criada no caminho indicado | Examine o resultado do `dotnet publish` e a configuração do `.csproj`. |
| `seu_arquivo.dbc` não foi encontrado | Confira o nome, a extensão e a pasta de entrada. Prefira caminhos completos entre aspas. |
| Não é possível substituir o `.exe` antigo | Feche o TabWon e qualquer processo que esteja utilizando o módulo; depois tente novamente. |
| A compilação falha após uma atualização | Como último recurso, feche os programas que utilizam os arquivos e execute `rmdir /s /q bin` e `rmdir /s /q obj` dentro da pasta do projeto; depois repita os comandos do passo 5. |

---

## 2. O que o TabWonDBC faz

O `.dbc` é um formato de arquivo dBase (`.dbf`) compactado com o algoritmo **PKWare DCL implode**, utilizado na disponibilização de tabelas compatíveis com o TabWin. O TabWonDBC converte diretamente esse formato para **Apache Parquet**, apropriado para armazenamento e processamento analítico de dados em colunas.

**Recursos documentados da implementação:**

- **Conversão direta:** não precisa criar um `.dbf` intermediário em disco.
- **Processamento em blocos:** reduz a necessidade de manter todos os registros descompactados simultaneamente na memória.
- **Execução em etapas sobrepostas:** descompactação, conversão de campos e gravação podem trabalhar simultaneamente.
- **Paralelismo:** utiliza múltiplas threads na conversão e codificação das colunas.
- **Modo lote:** processa vários arquivos ou uma pasta inteira, com conversões paralelas.
- **Publicação autocontida:** na configuração padrão documentada, o Windows recebe um executável que dispensa a instalação separada do .NET Runtime.
- **Dependência NuGet declarada:** biblioteca [Snappier](https://www.nuget.org/packages/Snappier), para compressão Snappy da saída.

O desempenho depende do tamanho dos arquivos, da quantidade de colunas, da velocidade do armazenamento e dos recursos do equipamento. Consulte a [seção 9](#9-desempenho).

## 3. Requisitos e detalhes técnicos

### 3.1 Requisitos

| Finalidade | Requisito |
|---|---|
| Compilar no Windows | Windows x64, .NET 8 SDK e acesso às dependências NuGet. |
| Executar no Windows | Publicação `win-x64` autocontida, conforme as configurações do projeto; normalmente não requer instalar o .NET Runtime no destino. |
| Baixar o código | Repositório do projeto no **GitHub**; o download por ZIP dispensa o Git instalado. |
| Editar o código (opcional) | Editor de texto, VS Code, Visual Studio ou outra IDE compatível com C#. |
| Outras plataformas (opcional) | Podem exigir publicação específica e validação de compatibilidade. Veja a [seção 4.3](#43-outros-sistemas-operacionais-opcional). |

### 3.2 Especificações

| Item | Descrição |
|---|---|
| Linguagem | C# 12 |
| *Framework* de destino | .NET 8 (`net8.0`) |
| Tipo de projeto | Aplicativo de console (`OutputType=Exe`) |
| Destino padrão documentado | Windows x64 (`win-x64`) |
| Dependência NuGet | `Snappier` 1.1.1 |
| Acesso de baixo nível | Blocos `unsafe` (`AllowUnsafeBlocks`), utilizados no processamento por ponteiros |
| Publicação pretendida | `SelfContained`, `PublishSingleFile` e `PublishReadyToRun` |
| Globalização | `InvariantGlobalization=true` |
| Entrada | `.dbc`: cabeçalho dBase III, 4 bytes de CRC e fluxo PKWare DCL implode |
| Saída | Apache Parquet, formato versão 1, compressão Snappy e metadados em Thrift compact protocol |
| Codificação de texto | Leitura Latin-1 (ISO-8859-1) e gravação UTF-8 |

Estas características descrevem a implementação documentada. Para saber quais opções estão ativas **na versão baixada**, confira `TabWonDBC.csproj` e o código-fonte correspondente.

## 4. Compilação e publicação: referência técnica

Esta seção complementa o [guia rápido](#1-guia-rápido-compilar-e-usar-no-windows) para desenvolvedores que desejam modificar ou integrar o módulo.

### 4.1 Restaurar e publicar

No diretório que contém `TabWonDBC.csproj`:

```bat
dotnet restore
dotnet publish -c Release
```

O primeiro comando restaura as dependências NuGet. O segundo compila e publica usando a configuração do `.csproj`.

Caminho previsto para a publicação Windows x64:

```text
bin\Release\net8.0\win-x64\publish\TabWonDBC.exe
```

Se estiver usando uma versão modificada do projeto, confirme o destino no resultado do comando, pois as propriedades de publicação podem ter sido alteradas.

### 4.2 Publicar explicitamente para Windows x64

Para informar o identificador de runtime (RID) de maneira explícita:

```bat
dotnet publish -c Release -r win-x64
```

Esse comando não substitui as demais propriedades de publicação configuradas no projeto. Para preservar o comportamento autocontido e em arquivo único, mantenha também essas opções no `.csproj` ou defina-as conscientemente na chamada.

### 4.3 Outros sistemas operacionais (opcional)

A publicação para Linux ou macOS poderá ser tentada com o respectivo RID, desde que o código e as configurações do projeto sejam compatíveis:

```bash
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r osx-arm64
dotnet publish -c Release -r osx-x64
```

Esses destinos correspondem a Linux x64, macOS Apple Silicon e macOS Intel, respectivamente. O arquivo gerado e o diretório variam por plataforma. **Este README não atesta a execução ou os testes nesses sistemas**, pois eles não integram o procedimento principal de distribuição para Windows.

### 4.4 Limpeza de arquivos de compilação

Somente se houver necessidade de refazer uma publicação do zero, e **apenas na pasta correta do projeto**, execute no Prompt do Windows:

```bat
rmdir /s /q bin
rmdir /s /q obj
dotnet restore
dotnet publish -c Release
```

Os comandos `rmdir` apagam permanentemente o conteúdo dessas pastas de compilação. Não os execute em outro diretório.

## 5. Como usar o executável

### 5.1 Converter um arquivo

Sintaxe:

```text
TabWonDBC.exe <entrada.dbc> <saida.parquet> [opcoes]
```

Exemplo, com arquivos na pasta atual:

```bat
TabWonDBC.exe seu_arquivo.dbc seu_arquivo.parquet
```

Exemplo, informando os caminhos completos:

```bat
TabWonDBC.exe "C:\dados\seu_arquivo.dbc" "C:\dados\seu_arquivo.parquet" --tempo
```

A conversão cria um arquivo Parquet com o nome e no local especificados como segundo argumento.

### 5.2 Converter vários arquivos (modo lote)

Sintaxe:

```text
TabWonDBC.exe --lote <pasta_saida> <entrada_1> [entrada_2 ...] [opcoes]
```

Exemplos:

```bat
TabWonDBC.exe --lote C:\parquet C:\dados\seu_arquivo.dbc C:\dados\seu_segundo_arquivo.dbc
TabWonDBC.exe --lote C:\parquet C:\dados
TabWonDBC.exe --lote C:\parquet "C:\dados\*.dbc" --jobs 4
```

- Cada `.dbc` é convertido para um `.parquet` com o mesmo nome-base, na pasta de saída.
- Quando a entrada é uma pasta, o programa procura arquivos `.dbc` nela, conforme a rotina de expansão implementada em `Program.cs`.
- Curingas como `*` e `?` são expandidos pelo próprio programa. Coloque padrões com curinga entre aspas no Prompt do Windows.
- `--jobs 4` limita a quatro o número de arquivos convertidos simultaneamente no lote.

### 5.3 Opções de linha de comando

| Opção | Padrão documentado | O que faz |
|---|---|---|
| `--tempo` | Desativado | Mostra tempos da descompactação, conversão e gravação, além das contagens de registros. |
| `--rowgroup N` | `1000000` | Define a quantidade de linhas por *row group* do Parquet. `0` indica um único grupo com o arquivo completo. |
| `--threads N` | Número de núcleos | Define quantas threads são utilizadas na conversão de cada arquivo. |
| `--lote <pasta>` | — | Ativa o modo lote e informa a pasta de saída. |
| `--jobs N` | Núcleos ÷ 2 | Define quantos arquivos podem ser processados ao mesmo tempo no modo lote. |
| `--versao` | — | Exibe informações de versão, autoria e licença. |
| `-h`, `--help` | — | Exibe a ajuda do programa. |

### 5.4 Mensagens e códigos de retorno

| Situação | Saída esperada | Código de saída |
|---|---|---|
| Conversão concluída | `OK: seu_arquivo.parquet` em `stdout` | `0` |
| Argumentos inválidos | Instruções de uso em `stderr` | `1` |
| Erro na conversão | `ERRO: <mensagem>` em `stderr` | `2` |
| Lote com uma ou mais falhas | `ERRO: <entrada>: <mensagem>` por falha | `2` |

No Prompt do Windows, é possível consultar o código retornado pelo último comando com `echo %ERRORLEVEL%`. De acordo com o comportamento documentado, um arquivo Parquet incompleto é apagado se a conversão falhar.

### 5.5 Exemplo de integração com Python

O Python pode chamar o programa externo e, depois, ler o Parquet gerado:

```python
import subprocess
import pandas as pd

resultado = subprocess.run(
    [r"C:\TabWonDBC\bin\Release\net8.0\win-x64\publish\TabWonDBC.exe",
     r"C:\dados\seu_arquivo.dbc",
     r"C:\dados\seu_arquivo.parquet"],
    capture_output=True,
    text=True,
)

if resultado.returncode != 0:
    raise RuntimeError(resultado.stderr)

df = pd.read_parquet(r"C:\dados\seu_arquivo.parquet")
```

O exemplo exige Python e os pacotes `pandas` e um mecanismo de leitura de Parquet, como `pyarrow`; eles **não são necessários** para usar o `TabWonDBC.exe` diretamente.

## 6. Funcionamento interno

### 6.1 Organização do arquivo `.dbc`

Conforme o formato tratado pela implementação:

```text
┌────────────────────────────┬──────────────┬─────────────────────────────┐
│ Cabeçalho dBase III        │ 4 bytes      │ Registros compactados       │
│ (campos, tipos, tamanhos   │ (CRC)        │ (PKWare DCL implode)        │
│ e contagem de registros)   │              │                             │
└────────────────────────────┴──────────────┴─────────────────────────────┘
```

Depois da descompactação, os registros têm tamanho fixo. O primeiro byte indica se o registro foi marcado como excluído (`*`); os campos seguintes ocupam posições definidas pelo cabeçalho.

### 6.2 Pipeline de conversão

O arquivo de entrada é **mapeado na memória**, permitindo que a leitura gerenciada pelo sistema operacional e as etapas de processamento se sobreponham. O pipeline descrito pela implementação organiza o trabalho assim:

```text
       .dbc (entrada)
             │
             ▼
  ┌────────────────────┐      ┌──────────────────────┐      ┌──────────────────────┐
  │ 1. DESCOMPACTAÇÃO  │ ───► │ 2. CONVERSÃO         │ ───► │ 3. GRAVAÇÃO          │
  │ BlastDecoder       │      │ bytes para colunas   │      │ codificação + Snappy │
  │ 1 thread           │      │ várias threads       │      │ várias threads       │
  └────────────────────┘      └──────────────────────┘      └──────────────────────┘
                                                                      │
                                                                      ▼
                                                               .parquet (saída)
```

1. **Descompactação:** o fluxo implode é sequencial e utiliza uma thread. Os registros são produzidos em blocos de aproximadamente 8 MB, preservando a janela de até 4 KB necessária ao algoritmo.
2. **Conversão:** registros marcados como excluídos são descartados. As colunas são distribuídas entre threads, com processamento em sub-blocos de 2.048 linhas e conversão diretamente dos bytes para os tipos de destino.
3. **Gravação:** quando um *row group* fica completo (1 milhão de linhas, por padrão), as colunas são codificadas e comprimidas em paralelo. Ao final, o programa grava os metadados de rodapé do Parquet.

Segundo a descrição técnica original, o pipeline mantém no máximo **quatro blocos descompactados** e **dois row groups** simultaneamente na memória para controlar o consumo de recursos.

### 6.3 Principais otimizações

| Técnica | Implementação | Objetivo |
|---|---|---|
| Tabela Huffman para símbolos de até 13 bits | `BlastDecoder` | Evitar a decodificação bit a bit em cada símbolo. |
| Buffer de bits de 64 bits | `BlastDecoder.Decode` | Reduzir leituras e desvios durante a descompactação. |
| Cópias de 8 bytes | `BlastDecoder.Decode` | Acelerar cópias de trechos repetidos. |
| Dicionário por coluna de texto | `DictStringColumn` | Armazenar cada valor textual distinto uma vez, referenciado por um identificador. |
| Comparação de campos curtos como inteiros de 64 bits | `DictStringColumn.GetIdRaw` | Acelerar a identificação de valores repetidos. |
| Conversão numérica diretamente de bytes | `Cell.ParseInt64`, `Cell.ParseDouble` | Evitar alocações de `string` no caminho comum. |
| Codificação própria de Parquet, com colunas em paralelo | `ChunkEncoder`, `PqFileWriter` | Aproveitar as estruturas já montadas durante a conversão. |
| Mapeamento do arquivo de entrada na memória | `Converter.Convert` | Sobrepor leitura de disco e processamento. |

### 6.4 Regras de conversão dos campos

A implementação documentada procura reproduzir as regras de leitura de valores utilizadas pelo componente `DbcToDbf`:

| Tipo dBase de entrada | Condição | Representação no Parquet | Valor vazio ou inválido |
|---|---|---|---|
| `C` e outros tratados como texto | — | Texto UTF-8 | `""` |
| `N` ou `F` | Zero casas decimais | Inteiro de 64 bits | `0` |
| `N` ou `F` | Uma ou mais casas decimais | `DOUBLE` | `0.0` |
| `D` (`AAAAMMDD`) | — | Data em `INT96` | `0001-01-01` |
| `L` | — | Booleano | `false` |

Detalhamento das regras:

- **Texto:** caracteres lidos como Latin-1 e espaços das extremidades removidos seguindo o comportamento do `string.Trim()` do .NET.
- **Inteiros:** aceitam espaços e sinais `+` e `-` à esquerda. Conteúdos inválidos, com ponto decimal ou fora do intervalo de 64 bits são convertidos em `0`.
- **Decimais:** aceitam sinais à esquerda ou à direita (como `12-`), vírgula para agrupamento de milhares e ponto decimal. Não aceitam notação científica, como `1e3`.
- **Booleanos:** `T`, `t`, `Y` e `y` são interpretados como `true`; outros valores, inclusive vazios, tornam-se `false`.
- **Registros excluídos:** entradas marcadas com `*` no início do registro não são incluídas na saída.
- **Arquivo truncado:** conforme o comportamento descrito na versão original, podem ser gravados os registros disponíveis quando o fluxo termina antes da quantidade informada no cabeçalho; problemas detectados pelo descompactador podem gerar erro.

## 7. Estrutura do projeto e funções

A estrutura-base documentada contém estes dez arquivos:

```text
TabWonDBC/
├── TabWonDBC.csproj   Configuração do projeto .NET 8
├── Program.cs         Interface de linha de comando e modo lote
├── Converter.cs       Pipeline de conversão
├── Blast.cs           Descompactador PKWare DCL implode
├── Dbf.cs             Cabeçalho dBase e conversão dos campos
├── Columns.cs         Estruturas de armazenamento das colunas
├── Parquet.cs         Codificador e gravador de Apache Parquet
├── README.md          Documentação de instalação, uso e implementação
├── LICENSE            Texto integral da GNU GPL v3
└── AVISO.md           Avisos de autoria e termos adicionais
```

### 7.1 `Program.cs` — classe `Program`

| Função | Responsabilidade |
|---|---|
| `Main(string[] args)` | Ponto de entrada. Analisa os argumentos, escolhe entre conversão individual e lote, chama `Converter.Convert`, imprime mensagens e devolve o código de retorno. No lote, utiliza `Parallel.ForEach` para processar até `--jobs` entradas simultaneamente. |
| `ExpandInputs(List<string> items)` | Expande entradas informadas como arquivos, diretórios ou curingas; remove duplicatas e ordena a lista resultante. |

### 7.2 `Converter.cs` — opções e pipeline

**`ConvertOptions`**: reúne as opções `RowGroupSize`, `Threads` e `Timing`.

| Função | Responsabilidade |
|---|---|
| `Convert(dbcPath, parquetPath, opt)` | Valida a entrada, mapeia o arquivo `.dbc` em memória (`MemoryMappedFile`) e inicia o processamento. |
| `Run(...)` | Monta o pipeline com buffers e filas (`BlockingCollection`), coordena as etapas, trata falhas e apresenta os tempos quando solicitado. |
| `DecodeInto(dec, buf, pos, limit)` | Prepara o buffer em memória e chama o descompactador. |
| `NewGroup(fields, plain, cap)` | Cria um *row group* com as estruturas de colunas adequadas aos tipos dos campos. |
| `UpdatePlain(g, plain)` | Identifica colunas de texto com alta diversidade de valores e ajusta a estratégia de armazenamento para os grupos seguintes. |
| `PartitionColumns(fields, n)` | Divide as colunas entre threads, considerando um custo estimado de processamento. |
| `ParseRows(...)` | Converte registros por coluna em sub-blocos de 2.048 linhas. |

Classes auxiliares documentadas: `Chunk` (bloco de dados), `Group` (grupo em construção) e `Stats` (tempos e contagens).

### 7.3 `Blast.cs` — classe `BlastDecoder`

Implementa a descompactação PKWare DCL **implode** com base no `blast.c` da biblioteca zlib, de Mark Adler.

| Membro | Responsabilidade |
|---|---|
| `BlastDecoder(byte* input, int offset, int length)` | Lê e valida os parâmetros iniciais do fluxo compactado, inclusive o modo de literais e o tamanho da janela. |
| `Decode(byte* o, int pos, int limit)` | Descompacta o fluxo por etapas e informa a posição de saída. Pode lançar `InvalidDataException` em condições inválidas. |
| `BuildTable(byte[] rep)` | Prepara as tabelas de decodificação de 8.192 entradas. |
| `Finished`, `TotalOut` | Informam o término do fluxo e a quantidade de bytes produzidos. |
| `Window`, `MaxOverrun` | Constantes documentadas para janela de 4.096 bytes e folga adicional de 600 bytes. |

### 7.4 `Dbf.cs` — tipos e leitura de campos

- **`ColKind`**: tipos de saída (`String`, `Int64`, `Double`, `DateTime` e `Bool`).
- **`DbfField`**: descrição do campo (nome, tipo original, tamanho, casas decimais, posição e tipo de destino).
- **`DbfHeader.Parse(ReadOnlySpan<byte> b)`**: leitura e validação da estrutura do cabeçalho e dos descritores de campo, até o terminador `0x0D`.

| Função de `Cell` | Responsabilidade |
|---|---|
| `Trim(p, w, out start)` | Identifica o trecho do conteúdo sem espaços nas extremidades. |
| `MakeString(p, w)` | Constrói texto Latin-1 sem espaços nas extremidades. |
| `ParseInt64(p, w)` | Converte bytes em inteiros, com caminho rápido e alternativa via `long.TryParse`. |
| `ParseDouble(p, w)` | Converte bytes em valores decimais, com caminhos otimizados e alternativa via `decimal.TryParse`. |
| `ParseDate(p, w)` | Interpreta datas em `AAAAMMDD`, validando seus componentes. |
| `ParseBool(p, w)` | Interpreta `T`/`t`/`Y`/`y` como verdadeiro. |

### 7.5 `Columns.cs` — representação em memória

| Classe | Responsabilidade |
|---|---|
| `ColumnData` | Classe-base, com preparação do espaço em memória via `Prepare(capacity)`. |
| `Int64Column`, `DoubleColumn`, `DateColumn`, `BoolColumn` | Vetores tipados que armazenam os valores dos campos. |
| `DictStringColumn` | Guarda textos distintos em dicionário, com identificadores por linha (`Ids`, `Keys`, `KeyStart`, `Count`). |
| `PlainStringColumn` | Armazena valores textuais diretamente em bytes UTF-8, sem dicionário. |
| `Latin1` | Rotinas de conversão de caracteres de Latin-1 para UTF-8. |

Funções relevantes:

| Membro | Responsabilidade |
|---|---|
| `DictStringColumn.GetIdRaw(p, w, mask0, mask1)` | Busca rápida para campos de até 16 bytes. |
| `DictStringColumn.AddRaw(...)` | Normaliza o conteúdo e registra valores ainda não presentes. |
| `DictStringColumn.GetId(p, n)` | Recupera ou cria o identificador de um texto. |
| `Hash`, `Add`, `Rehash`, `Put`, `InitRaw` | Implementação das tabelas *hash* de endereçamento aberto. |
| `PlainStringColumn.Append(row, p, n)` | Acrescenta um valor textual à estrutura de armazenamento. |
| `Latin1.ToUtf8(s, n, d)` | Converte os bytes Latin-1 para UTF-8. |
| `Latin1.Utf8Length(s, n)` | Calcula o tamanho necessário em UTF-8. |

### 7.6 `Parquet.cs` — gravação do arquivo Parquet

**`PqFileWriter`** organiza os grupos de linhas, a estrutura de colunas e os metadados finais.

| Função | Responsabilidade |
|---|---|
| `PqFileWriter(Stream, fields)` | Inicia o arquivo com a assinatura `PAR1`. |
| `WriteRowGroup(rows, chunks)` | Grava as colunas codificadas e registra as respectivas posições. |
| `Finish()` | Grava *schema*, grupos, estatísticas, tamanho do rodapé e assinatura final `PAR1`. |
| `PhysicalType(ColKind)` | Define o tipo físico de cada coluna no formato Parquet. |

**`ChunkEncoder`** prepara as páginas de dados e aplica a compressão Snappy.

| Função | Responsabilidade |
|---|---|
| `Encode(col, field, rows)` | Seleciona a rotina de codificação para o tipo de coluna. |
| `EncodeDict` | Codifica textos utilizando uma página de dicionário e identificadores em *bit-packing*. |
| `EncodeDictAsPlain`, `EncodePlainStrings` | Codificam textos diretamente em formato PLAIN. |
| `EncodeFixed` | Codifica inteiros e valores `DOUBLE` em 8 bytes por registro. |
| `EncodeDates` | Codifica datas em `INT96` (dia juliano e nanossegundos do dia). |
| `EncodeBools` | Codifica valores booleanos em 1 bit por registro. |
| `Stats` | Prepara estatísticas de mínimo e máximo nos metadados. |
| `WritePage` | Aplica Snappy e grava o cabeçalho e o conteúdo da página. |
| `WriteAllDefined`, `WriteVarint`, `BitPack` | Implementam elementos de definição, inteiros variáveis e empacotamento de bits. |

O limite documentado é de **131.072 valores por página de dados** (`PageRows`). A classe **`Thrift`** implementa um subconjunto do *Thrift compact protocol* para os metadados do Parquet: inteiros (`I8`, `I16`, `I32`, `I64`), booleanos, binários, strings, listas e estruturas. **`EncodedChunk`** contém a coluna codificada e seus metadados.

## 8. Formato de saída: Apache Parquet

| Campo de origem | Tipo físico Parquet | Tipo lógico | Obrigatoriedade |
|---|---|---|---|
| Texto | `BYTE_ARRAY` | `STRING` (UTF-8) | Opcional na definição; na prática, produzido sem nulos |
| Inteiro | `INT64` | Inteiro assinado de 64 bits | Obrigatório |
| Decimal | `DOUBLE` | — | Obrigatório |
| Data | `INT96` | Sem anotação lógica; interpretado por leitores como *timestamp* | Obrigatório |
| Lógico | `BOOLEAN` | — | Obrigatório |

- **Compressão:** Snappy.
- **Row groups:** por padrão, 1.000.000 de linhas, configuráveis por `--rowgroup`.
- **Texto:** dicionário quando vantajoso; codificação PLAIN para colunas de alta cardinalidade.
- **Nomes e ordem das colunas:** preservados a partir do cabeçalho do arquivo de origem.

A documentação original registra utilização com pandas (PyArrow e fastparquet), Polars e DuckDB.

### Atenção às datas vazias

Na implementação documentada, datas de entrada vazias ou inválidas podem se tornar `0001-01-01`. Alguns leitores que representam datas como *timestamps* de nanossegundos possuem uma faixa de anos limitada e podem interpretar esse valor de maneira inesperada.

Ao analisar esses arquivos, confira os valores de data no leitor escolhido e trate os valores sentinela conforme a necessidade do seu projeto. Por exemplo, caso a coluna já esteja representada como data e seja compatível com a operação:

```python
df.loc[df["sua_coluna_data"].dt.year < 1800, "sua_coluna_data"] = pd.NaT
```

O tratamento acima é apenas ilustrativo: depende do tipo de dado efetivamente retornado pela biblioteca de leitura e da semântica da tabela analisada.

## 9. Desempenho

A documentação técnica original apresenta o seguinte **ensaio de referência**, realizado em uma máquina Linux com **2 núcleos**, utilizando um arquivo com **2 milhões de registros**, **61 colunas**, cerca de **810 MB de conteúdo DBF** e **245 MB em DBC**:

| Métrica | Processo em duas etapas (DbcToDbf + Parquet.Net) | TabWonDBC |
|---|---:|---:|
| Tempo de conversão | 175 s | **3,4 s** |
| Pico de memória | ~4,4 GB | **~0,6 GB** |
| Tamanho do Parquet gerado | 134 MB | **118 MB** |

**Esses números foram preservados da documentação fornecida; não representam um novo teste executado durante esta revisão.** Os resultados podem variar conforme o arquivo, a plataforma e o equipamento utilizado.

Em máquinas com mais núcleos, a conversão de colunas e a gravação podem ganhar desempenho, embora a descompactação sequencial continue sendo uma etapa limitante. Para conjuntos com vários arquivos, o modo `--lote` permite paralelizar conversões independentes.

Para medir um arquivo no seu computador, utilize:

```bat
TabWonDBC.exe seu_arquivo.dbc seu_arquivo.parquet --tempo
```

Exemplo ilustrativo do formato de relatório de duração:

```text
[tempo] seu_arquivo.dbc: total 3361 ms | descompactação 2957 ms | conversão 2591 ms | gravação 1175 ms | 2,000,000 registros (0 apagados) | 61 colunas
```

Os tempos das etapas podem se sobrepor, portanto sua soma pode ultrapassar a duração total.

## 10. Limitações conhecidas

Conforme a documentação da implementação:

- **Tamanho do DBC:** arquivos de entrada acima de **2 GB** não são aceitos pela versão descrita.
- **Campos *memo* (`M`):** são interpretados como texto contido no próprio registro, normalmente uma referência a bloco; o conteúdo externo de `.dbt` e `.fpt` não é lido.
- **Textos extensos:** campos textuais acima de 255 caracteres, em extensões Clipper/FoxPro, não são tratados.
- **Precisão numérica:** números com 19 ou mais dígitos significativos podem produzir pequenas diferenças de arredondamento na representação `DOUBLE` quando comparados com outros conversores.
- **CRC do DBC:** os 4 bytes de CRC não são verificados. O descompactador detecta certas condições de truncamento ou corrupção do fluxo, mas isso não equivale a uma validação completa de integridade.

Antes de utilizar os arquivos convertidos em fluxos críticos, valide uma amostra dos resultados, incluindo número de linhas, tipos de colunas e valores esperados.

## 11. Perguntas frequentes

### Preciso instalar o .NET para utilizar o executável publicado?

**Não**, se o projeto tiver sido publicado como **autocontido** (`SelfContained=true`) para a plataforma correta. O SDK é necessário para compilar; o Runtime não precisa ser instalado separadamente para executar essa publicação.

### Por que o `.exe` é grande?

Uma publicação autocontida e em arquivo único inclui componentes do .NET no pacote. Em cenários em que a instalação do Runtime é garantida no destino, é possível optar por uma publicação dependente do *framework*:

```bat
dotnet publish -c Release -p:SelfContained=false -p:PublishSingleFile=false
```

**Atenção:** nesse caso, poderão ser produzidos vários arquivos e será necessária uma instalação compatível do .NET Runtime no computador de destino. Esse não é o procedimento recomendado neste README para atualizar o módulo do TabWon.

### O TabWonDBC converte arquivos `.dbf` não compactados?

Não diretamente. A interface descrita foi desenvolvida para receber arquivos `.dbc`.

### Posso usar o código em outro projeto C#?

A implementação pode ser adaptada para integração como código-fonte, mantendo os avisos de autoria e respeitando a licença. Uma chamada de referência dentro do projeto é:

```csharp
Converter.Convert(entrada, saida, new ConvertOptions());
```

As classes são documentadas como `internal`; para uso entre *assemblies*, seria necessário adaptar a visibilidade ou a organização do código. **A redistribuição de código incorporado a outro programa deve respeitar as obrigações da GPL-3.0 e os termos adicionais aplicáveis**, descritos em [AVISO.md](AVISO.md).

### O Parquet gerado é idêntico, byte a byte, ao de outros conversores?

Não necessariamente. Mesmo quando os valores lógicos correspondem, podem existir diferenças de codificação, organização em *row groups*, uso de dicionários e metadados, como `created_by`.

### Como obtenho a versão atual do código-fonte?

Consulte o **repositório do projeto no GitHub**. Confirme se está utilizando a revisão desejada antes de compilar. A documentação deste arquivo deve ser versionada junto do código e dos avisos de licença.

## 12. Créditos, licença e avisos

### 12.1 Componentes e referências

- **Descompactação PKWare DCL implode:** implementação baseada no [`blast.c` de Mark Adler](https://github.com/madler/zlib/tree/develop/contrib/blast), distribuído com a [licença zlib](https://zlib.net/zlib_license.html). O código em `Blast.cs` é descrito como uma reescrita modificada em C#, devendo preservar os avisos de origem aplicáveis.
- **Compressão Snappy:** [Snappier](https://github.com/brantburnett/Snappier), de Brant Burnett, disponível no NuGet. Consulte a licença do componente em sua distribuição oficial.
- **Formato de saída:** [especificação do Apache Parquet](https://parquet.apache.org/docs/file-format/).
- **Ferramentas de compilação:** [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0), da Microsoft.

### 12.2 Licença do TabWonDBC

Copyright (C) 2026 **Thiago Rodrigues (thiagusr)**.

O TabWonDBC é software livre distribuído sob a **GNU General Public License, versão 3**, identificada como `GPL-3.0-only`. O arquivo [LICENSE](LICENSE) contém o texto integral da licença. O projeto também inclui um [aviso de atribuição de autoria](AVISO.md), nos termos da seção 7(b) da GPL-3.0.

De forma resumida, a licença permite usar, estudar, modificar e redistribuir o programa, inclusive comercialmente, desde que as obrigações aplicáveis sejam cumpridas. A distribuição de versões modificadas deve identificar as alterações, preservar os avisos pertinentes e disponibilizar o código-fonte correspondente na forma exigida pela GPL. O programa é fornecido **sem garantia**, conforme o texto da licença.

**Ao distribuir o executável de forma independente**, disponibilize também o texto da licença, os avisos exigidos e o código-fonte correspondente. **Na integração com o TabWon**, mantenha essas informações e a disponibilidade do código na distribuição do projeto principal.

Para visualizar as informações do programa pela linha de comando:

```bat
TabWonDBC.exe --versao
```

### 12.3 Relação com o TabWin e o DATASUS

O **TabWin** é mantido pelo **DATASUS**. O **TabWon** e seu módulo **TabWonDBC** são projetos distintos e não devem ser confundidos com o programa oficial. A utilização de arquivos `.dbc` compatíveis com esse ecossistema não implica vínculo institucional ou endosso oficial do DATASUS.

---

**Nota de manutenção:** as descrições de classes, funções, desempenho, comportamento e opções de publicação devem permanecer sincronizadas com a versão efetivamente publicada no repositório GitHub. Alterações futuras no código podem exigir atualização deste README.

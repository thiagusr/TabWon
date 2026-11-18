# Aviso de autoria e termos adicionais

**Projeto:** TabWonDBC — conversor de arquivos `.dbc` para Apache Parquet, integrante do projeto TabWon.  
**Autor original:** Thiago Rodrigues (thiagusr).  
**Copyright (C) 2026 Thiago Rodrigues (thiagusr).**

O **TabWonDBC** é software livre, licenciado sob a **GNU General Public License, versão 3** (`GPL-3.0-only`). O texto integral da licença está disponível no arquivo [LICENSE](LICENSE), que deve acompanhar as distribuições aplicáveis.

Este documento registra os avisos de autoria, o termo adicional previsto na licença e os créditos a componentes de terceiros. Deve ser mantido acessível no **repositório GitHub** junto do código-fonte e da licença.

**Repositório oficial:** [https://github.com/thiagusr/tabwon](https://github.com/thiagusr/tabwon)

O TabWonDBC integra o projeto TabWon, desenvolvido por Thiago Rodrigues (thiagusr). O código-fonte e a documentação estão disponíveis publicamente no repositório oficial do projeto no GitHub.

## 1. Termo adicional de autoria — GPL-3.0, seção 7(b)

Nos termos da **seção 7(b) da GNU GPL v3**, o projeto é distribuído com o seguinte termo adicional de atribuição, aplicável às cópias, modificações e redistribuições do código ou de partes dele, observados os limites da própria licença:

1. **Preservação de avisos nos arquivos-fonte.** O cabeçalho de autoria existente no início de cada arquivo-fonte deve ser preservado nas cópias e versões modificadas daquele arquivo. O aviso identifica o programa original **TabWonDBC** e seu autor, **Thiago Rodrigues (thiagusr)**.

2. **Avisos legais apresentados pelo programa.** Quando versões modificadas exibirem avisos legais (*Appropriate Legal Notices*, na terminologia da GPL-3.0), inclusive na ajuda (`--help`), na opção `--versao` ou em uma tela “Sobre”, esses avisos devem continuar a indicar que o programa se baseia no **TabWonDBC**, de **Thiago Rodrigues (thiagusr)**.

3. **Incorporação em outros programas.** Se partes do código-fonte forem incorporadas a outro programa, os avisos de origem e autoria correspondentes deverão ser mantidos nos arquivos que contenham o material incorporado.

Este termo **não proíbe** que contribuidores ou mantenedores de versões modificadas acrescentem seus próprios avisos de autoria e copyright, desde que não eliminem atribuições cuja preservação seja exigida pela licença.

Nos termos da **seção 5(a) da GPL-3.0**, versões modificadas distribuídas também devem informar de modo destacado que sofreram alterações e indicar a data pertinente à modificação.

Para consulta, veja a [GPL-3.0 integral](LICENSE) e o [texto oficial da licença na Free Software Foundation](https://www.gnu.org/licenses/gpl-3.0.html).

## 2. Aplicação na distribuição

- **Módulo independente:** distribua o `TabWonDBC.exe` acompanhado das informações e dos arquivos de licença e autoria aplicáveis (`LICENSE` e `AVISO.md`), da documentação (`README.md`) e do acesso ao código-fonte correspondente, conforme a GNU GPL v3.
- **Módulo integrado ao TabWon:** mantenha os avisos e a documentação pertinente na distribuição do projeto principal. A integração não elimina as obrigações de atribuição, licença e disponibilização do código-fonte correspondente.
- **Versão modificada:** identifique as alterações e preserve os avisos exigidos, distinguindo a autoria original das contribuições posteriores.

O **TabWin**, utilizado com tabelas compatíveis com arquivos `.dbc`, é mantido pelo **DATASUS**. Essa referência ao ecossistema de arquivos não representa vínculo institucional nem endosso oficial ao projeto TabWon ou ao TabWonDBC.

## 3. Componentes de terceiros

| Componente | Autor / origem | Licença | Utilização |
|---|---|---|---|
| Algoritmo e tabelas de [`blast.c`](https://github.com/madler/zlib/tree/develop/contrib/blast) (zlib/contrib) | Mark Adler | [Licença zlib](https://zlib.net/zlib_license.html) | Base do descompactador em `Blast.cs`, em uma implementação alterada e reescrita em C#. |
| [Snappier](https://github.com/brantburnett/Snappier) | Brant Burnett e contribuidores do projeto | Consulte a licença da versão distribuída | Dependência NuGet utilizada para compressão Snappy. |

A licença zlib é compatível com a GNU GPL v3. O aviso de origem do `blast.c` existente no início de `Blast.cs` deve ser preservado conforme seus termos.

## 4. Texto da licença e ausência de garantia

A GNU GPL v3 concede direitos e estabelece condições que não são integralmente reproduzidos neste aviso. Em caso de dúvida, consulte o texto integral em [LICENSE](LICENSE).

O software é disponibilizado **sem garantia**, nos termos estabelecidos pela GNU GPL v3.

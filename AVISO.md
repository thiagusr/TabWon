# Aviso de autoria, licença e termos adicionais

**TabWon** — Tabulador Livre de dados do SUS
Copyright (C) 2026 **Thiago Rodrigues** (thiagusr)
Repositório oficial: [https://github.com/thiagusr/TabWon](https://github.com/thiagusr/TabWon)

Este aviso vale para todo o projeto TabWon: o programa principal (`TabWon.py`), os arquivos de
configuração, idiomas, ajuda, paletas e ícones da pasta `TabWon\_configs`, os modelos de imagem da
pasta `Imagens`, os roteiros de compilação (`compilar.bat` e `TabWon.spec`), o **loader**
(pasta `loader`, que gera o `TabWon.exe`), a documentação e o programa companheiro
**TabWonDBC** (pasta `TabWonDBC`, que tem também o seu próprio [`AVISO.md`](TabWonDBC/AVISO.md)).

## Licença

O TabWon é **software livre**, licenciado sob a **GNU General Public License, versão 3**
(GPL-3.0-only). O texto completo está no arquivo [`LICENSE`](LICENSE).

Em resumo, a GPL-3.0 garante a qualquer pessoa o direito de **usar, estudar, copiar, modificar e
redistribuir** o programa, inclusive comercialmente, desde que:

* o código-fonte acompanhe (ou fique disponível junto com) qualquer cópia distribuída,
inclusive o executável compilado;
* as versões modificadas sejam distribuídas sob a mesma licença GPL-3.0;
* as versões modificadas informem que foram alteradas e a data da alteração (seção 5, alínea a);
* os avisos de licença e de autoria sejam mantidos.

O programa é distribuído **sem nenhuma garantia**, conforme as seções 15 e 16 da licença.

## Termo adicional de autoria (GPL-3.0, seção 7, alínea b)

A seção 7(b) da GPL-3.0 permite exigir que avisos de autoria sejam preservados. Com base nela,
o TabWon é distribuído com o seguinte termo adicional, que vale para qualquer pessoa que copie,
modifique ou redistribua o programa ou partes dele:

1. **Arquivos de aviso**: os arquivos `LICENSE` e `AVISO.md` devem acompanhar todas as cópias e
todas as versões modificadas, no código-fonte e nos pacotes compilados.
2. **Avisos exibidos pelo programa**: as versões modificadas devem manter, na tela **Sobre**
(arquivos `TabWon\_sobre\*.txt`) e em qualquer outro aviso legal exibido ("Appropriate Legal
Notices", na definição da GPL-3.0), a informação de que se baseiam no programa original
**TabWon**, de **Thiago Rodrigues**.
3. **Obras derivadas**: quem incorporar partes do TabWon em outro programa deve manter, na
documentação e nos avisos legais desse programa, a menção ao programa original (**TabWon**) e
ao autor (**Thiago Rodrigues**).
4. **TabWonDBC**: o conversor TabWonDBC segue o mesmo princípio e tem termo próprio, descrito em
[`TabWonDBC/AVISO.md`](TabWonDBC/AVISO.md).

Este termo **não impede** que quem modificar o programa acrescente os próprios avisos de autoria
e de copyright. Contribuições são bem-vindas e serão creditadas.

## Relação com o TabWin e o DATASUS

O TabWon é um projeto **independente**, inspirado no **TabWin**, e **não tem vínculo
institucional com o DATASUS nem com o Ministério da Saúde**, que não participam do
desenvolvimento, não o endossam e não respondem por ele.

O TabWon lê os mesmos formatos de arquivo usados pelo TabWin (DEF, CNV, DBF e DBC) para que quem
já trabalha com os dados do SUS possa usá-lo sem refazer nada. O nome "TabWin" e os formatos
pertencem aos seus respectivos titulares e são citados apenas como referência.

Fica o agradecimento ao Ministério da Saúde e ao DATASUS, que criaram o TabWin, definiram esses
formatos e disponibilizam as bases públicas que tornam este trabalho possível.

## Marcas e imagens

* O logotipo **TabWon** e os modelos de ícones da pasta `TabWon\_configs/icones` são parte do
projeto e seguem a mesma licença.
* A marca **SUS** pertence ao Ministério da Saúde, e a **Bandeira Nacional** é símbolo
nacional regido pela Lei nº 5.700/1971. As duas aparecem no programa apenas como
referência ao sistema público de saúde, sem indicar vínculo ou endosso.

## Dados

O TabWon funciona inteiramente no computador de quem o usa: **nenhum dado ou tabulação é
enviado para servidores externos**. Quem usa o programa é responsável pelo tratamento dos dados
que tabula, conforme a legislação aplicável, como a Lei Geral de Proteção de Dados (LGPD).

## Componentes de terceiros

O TabWon usa as bibliotecas abaixo. No executável compilado, elas ficam na pasta
`TabWon\_dependencias`, cada uma sob a sua própria licença, todas compatíveis com a GPL-3.0.

|Componente|Licença|Uso|
|-|-|-|
|[Python](https://www.python.org) e Tcl/Tk (tkinter)|PSF / BSD|Linguagem e interface gráfica|
|[pandas](https://pandas.pydata.org)|BSD-3-Clause|Tabulação e manipulação dos dados|
|[NumPy](https://numpy.org)|BSD-3-Clause|Cálculos numéricos|
|[Apache Arrow / pyarrow](https://arrow.apache.org)|Apache-2.0|Leitura e gravação do cache Parquet|
|[DuckDB](https://duckdb.org)|MIT|Consultor SQL|
|[openpyxl](https://openpyxl.readthedocs.io)|MIT|Exportação para Excel (XLSX)|
|[Matplotlib](https://matplotlib.org)|Matplotlib License (PSF)|Gráficos|
|[Pillow](https://python-pillow.org)|MIT-CMU|Imagens e logotipo|
|[pywin32](https://github.com/mhammond/pywin32)|PSF|Área de transferência do Windows|
|[PyInstaller](https://pyinstaller.org)|GPL-2.0 com exceção para o bootloader|Geração do executável|
|.NET Framework 4.8 (Microsoft)|Componente do sistema Windows|Base do loader (`TabWon.exe`); já vem no Windows e não é distribuído com o TabWon|

## Contato

Dúvidas, sugestões, relatos de problemas e contribuições: pelo repositório oficial do projeto,
[https://github.com/thiagusr/TabWon](https://github.com/thiagusr/TabWon) (issues e pull requests). Veja o [`README.md`](README.md).

A única fonte oficial do TabWon é esse repositório. Cópias ou versões
compiladas obtidas em outros locais não são oficiais.


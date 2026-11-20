# Como criar um modelo de ícones para o TabWon (GIMP)

Este guia explica como **personalizar, exportar e instalar modelos de ícones no TabWon** usando o [GIMP](https://www.gimp.org/). Os arquivos de exemplo já incluem o modelo **Linear Colorido** e podem ser utilizados como ponto de partida: basta editar os desenhos existentes ou substituí-los por outros, respeitando as posições e dimensões previstas.

> **Requisito:** GIMP 2.10 ou versão mais recente para abrir e editar os arquivos `.xcf`.

## Arquivos do pacote

| Arquivo | Conteúdo |
| --- | --- |
| `TabWon_icones_modelo.xcf` | Modelo editável dos ícones dos botões, com **192 × 864 pixels**. |
| `TabWon_alertas_modelo.xcf` | Modelo editável dos ícones de alertas e perguntas, com **192 × 48 pixels**. |
| `TabWon_icones_mapa.png` | Mapa ampliado com os nomes e as posições dos ícones na grade. |
| `LEIA-ME_modelo_icones.txt` | Instruções originais que acompanham o pacote. |
| `TabWon_logo.xcf` | Arquivo editável do logotipo do TabWon. |
| `TabWon_Brasil.xcf` | Arquivo editável de identidade visual mapa do Brasil no TabWon. |
| `TabWon_SUS.xcf` | Arquivo editável de identidade visual do SUS no TabWon. |
| `TabWon_icos.xcf` | Arquivo editável dos ícones de sistema do TabWon (bandeja, barras de títulos e afins). |

Os **dois modelos de ícones** (`TabWon_icones_modelo.xcf` e `TabWon_alertas_modelo.xcf`) podem ser abertos diretamente no GIMP e contêm camadas separadas para facilitar a criação de modelos personalizados. Os **três arquivos de identidade visual** listados acima têm finalidade distinta; veja a seção a seguir.

## Identidade visual oficial e contribuições

O **TabWon** é um projeto independente, distribuído como software livre sob a **[GNU General Public License, versão 3 (GPL v3)](https://www.gnu.org/licenses/gpl-3.0.html)**. O repositório original é **[github.com/thiagusr/TabWon](https://github.com/thiagusr/TabWon/)**.

Os arquivos `TabWon_Brasil.xcf`, `TabWon_logo.xcf`, `TabWon_SUS.xcf` e `TabWon_icos.xcf` são disponibilizados em formato **editável** para permitir consulta e manutenção dos elementos gráficos do projeto. Entretanto, **alterações nesses arquivos somente serão aceitas como parte da identidade visual oficial do TabWon após análise e aprovação no repositório original**. Não apresente uma versão alterada como sendo a versão oficial ou aprovada do projeto sem essa aprovação.

Para propor alterações, encaminhe a sugestão pelo [repositório oficial do TabWon](https://github.com/thiagusr/TabWon/), de forma que a proposta possa ser avaliada antes de eventual incorporação. A disponibilização dos arquivos `.xcf` não constitui, por si só, aprovação de versões modificadas para uso como identidade visual oficial.

**Distinção importante sobre a licença:** a exigência de aprovação acima se refere à **adoção e apresentação oficial** da identidade visual, e **não à proibição de modificar materiais cobertos pela GPL v3**. A GPL v3 assegura direitos de modificação e redistribuição de obras sob essa licença; sua seção 7 permite, entre outros pontos, exigir identificação de versões alteradas e preservar direitos de marca aplicáveis. Se esses arquivos gráficos estiverem sujeitos a condições de licenciamento próprias, essas condições deverão ser identificadas expressamente nos respectivos arquivos ou em um aviso de licença específico, sem contrariar os direitos já concedidos pela GPL para o material coberto por ela.

**Importante:** a criação de **modelos personalizados de ícones** a partir dos arquivos `TabWon_icones_modelo.xcf` e `TabWon_alertas_modelo.xcf`, conforme este guia, não significa a aprovação de alterações no logotipo ou na identidade visual oficial do projeto.

## 1. Arquivos que o TabWon utiliza

Cada modelo é composto por **três arquivos PNG**, que devem ficar na pasta `TabWon_configs\icones`:

| Arquivo | Finalidade |
| --- | --- |
| `<Nome>_claro.png` | Ícones dos botões no modo claro. |
| `<Nome>_escuro.png` | Ícones dos botões no modo escuro, incluindo o modo automático durante a noite. |
| `<Nome>_alertas.png` | Ícones das janelas de informação, aviso, erro e pergunta. |

Substitua `<Nome>` pelo nome escolhido para o modelo. Por exemplo, para um modelo chamado **Praia**, os arquivos serão `Praia_claro.png`, `Praia_escuro.png` e `Praia_alertas.png`.

**Observação:** se `<Nome>_escuro.png` não existir, o TabWon utilizará `<Nome>_claro.png` nos dois modos de aparência.

## 2. Editar os ícones dos botões

Abra `TabWon_icones_modelo.xcf` no GIMP.

### 2.1. Tamanho e organização da grade

- **Dimensões totais:** 192 × 864 pixels.
- **Grade:** 8 colunas × 36 linhas.
- **Tamanho de cada quadro:** 24 × 24 pixels.
- **Fundo:** transparente.
- **Posicionamento:** um ícone por quadro, sempre na ordem indicada em `TabWon_icones_mapa.png`.
- **Margem recomendada:** deixe de 1 a 2 pixels livres nas bordas de cada quadro.
- **Espaços reservados:** os quadros com um **X vermelho** na camada `Grade` devem permanecer vazios.

Para visualizar a grade durante a edição, ative as opções do GIMP:

1. **Visualizar → Mostrar guias**.
2. **Visualizar → Ajustar às guias**.

### 2.2. As três partes da imagem

A grade contém **três conjuntos de 12 linhas**, cada um com os mesmos 90 ícones, na mesma ordem. O TabWon seleciona automaticamente o conjunto apropriado conforme a cor de fundo do botão.

| Parte | Faixa vertical (`y`) | Como desenhar | Onde é utilizada |
| --- | --- | --- | --- |
| **A** | 0 a 287 px | Ícones para fundos claros ou neutros; podem ser coloridos. | Botões cinza, brancos ou de fundo claro/neutro. |
| **B** | 288 a 575 px | Os mesmos desenhos em **branco**. | Botões escuros ou de cores fortes, como azul, verde, vermelho e roxo. |
| **C** | 576 a 863 px | Os mesmos desenhos em cor **escura**, como `#263238`. | Botões de cores claras, como amarelo e laranja. |

**Importante:** mantenha os ícones correspondentes nas mesmas posições nas três partes. Não altere a dimensão da imagem nem a ordem dos quadros.

### 2.3. Calcular a posição de um ícone

As posições são calculadas a partir da coluna e da linha indicadas no mapa. A contagem de colunas e linhas começa em **1**.

```text
x = (coluna - 1) × 24

y na parte A = (linha - 1) × 24
y na parte B = (linha - 1) × 24 + 288
y na parte C = (linha - 1) × 24 + 576
```

**Exemplo — ícone “salvar”:** coluna 7, linha 4.

| Parte | Coordenada `x` | Coordenada `y` |
| --- | ---: | ---: |
| A | 144 px | 72 px |
| B | 144 px | 360 px |
| C | 144 px | 648 px |

Os ícones **XLSX** e **CSV** são representados, no modelo de exemplo, por uma folha com a extensão escrita. Se desejar, mantenha esses desenhos em todos os seus modelos.

## 3. Camadas do arquivo `.xcf`

O modelo possui as seguintes camadas:

| Camada | Função |
| --- | --- |
| `Grade` | Mostra os contornos dos quadros e os X vermelhos nas posições vazias. **Oculte antes de exportar.** |
| `Parte C - desenhos escuros (linhas 25-36)` | Desenhos escuros para botões de cores claras. |
| `Parte B - desenhos brancos (linhas 13-24)` | Desenhos brancos para botões escuros ou de cores fortes. |
| `Parte A - fundo claro/neutro (linhas 1-12)` | Desenhos para botões claros ou neutros. |
| `Fundo de referência` | Fundos cinza, azul e laranja para verificar o contraste. **Oculte antes de exportar.** |

Desenhe nas camadas das partes **A**, **B** e **C**. Você também pode criar outras camadas: na exportação para PNG, o GIMP combina todas as camadas visíveis.

## 4. Exportar os PNG dos botões

### 4.1. Exportar a versão para o modo claro

1. Oculte as camadas **`Grade`** e **`Fundo de referência`**, clicando no ícone de olho ao lado de cada uma.
2. No GIMP, acesse **Arquivo → Exportar como...**.
3. Informe o nome `<Nome>_claro.png` (por exemplo, `Praia_claro.png`) e escolha o formato **PNG**.
4. Nas opções de exportação, mantenha **Salvar cor de fundo** desmarcado para preservar a transparência.
5. Clique em **Exportar**.

### 4.2. Exportar a versão para o modo escuro

1. Ajuste as cores dos ícones da **parte A**, preferencialmente utilizando tons mais claros, pois os fundos dos botões neutros serão escuros.
2. As partes **B** e **C** normalmente podem permanecer iguais.
3. Exporte novamente, desta vez com o nome `<Nome>_escuro.png` (por exemplo, `Praia_escuro.png`).
4. Salve também uma cópia do projeto `.xcf` com outro nome, para poder editar o modelo posteriormente.

> **Atenção:** mantenha a imagem com **192 × 864 pixels**, incluindo as três partes. Não recorte, redimensione nem reorganize os ícones.

## 5. Criar e exportar os ícones de alerta

Abra `TabWon_alertas_modelo.xcf` no GIMP.

Esse arquivo possui **192 × 48 pixels**, divididos em **quatro quadros de 48 × 48 pixels**, dispostos horizontalmente e com fundo transparente.

| Posição | Tipo de ícone |
| --- | --- |
| 1º quadro | Informação |
| 2º quadro | Aviso |
| 3º quadro | Erro |
| 4º quadro | Pergunta |

Após editar os desenhos, oculte as camadas **`Grade`** e **`Fundo de referência`** e exporte o arquivo como `<Nome>_alertas.png` (por exemplo, `Praia_alertas.png`).

## 6. Instalar e selecionar o modelo no TabWon

1. Copie os três arquivos PNG exportados para a pasta `TabWon_configs\icones`.
2. Cadastre o nome do modelo na lista de modelos por uma destas alternativas:
   - **Pela interface:** pressione **Shift** enquanto clica na **engrenagem** e acesse **Configurações avançadas → Cores**.
   - **Pelo arquivo de configuração:** edite `TabWon_configs.ini`, na seção `[icones]`, acrescentando o nome à linha `modelos=`. Separe os nomes dos modelos por **ponto e vírgula** (`;`).
3. No TabWon, abra **Configurações → Aparência → Ícones**.
4. Selecione o modelo na lista.
5. Confira a **prévia** abaixo da lista, que mostra a aparência dos botões claros e coloridos.

O nome usado nos arquivos PNG deve corresponder ao nome cadastrado na lista de modelos.

## 7. Dicas para obter bons resultados

- **Teste em tamanho real:** no GIMP, use **Visualizar → Ampliação → 1:1**. Um ícone bonito quando ampliado pode ficar pouco legível com apenas 24 pixels.
- **Prefira traços nítidos:** espessuras em torno de **2 pixels** costumam funcionar bem; traços menores que aproximadamente **1,5 pixel** podem desaparecer visualmente.
- **Verifique o contraste:** use o fundo de referência durante a criação e oculte-o antes de exportar.
- **Respeite a ordem:** confira `TabWon_icones_mapa.png` sempre que tiver dúvidas sobre a posição ou o significado de um quadro.
- **Preserve o arquivo editável:** guarde uma cópia do `.xcf` para futuras alterações.

---

**Projeto TabWon — repositório original:** [github.com/thiagusr/TabWon](https://github.com/thiagusr/TabWon/)

Este guia documenta a criação de modelos de ícones personalizados para o TabWon com base nos arquivos de modelo fornecidos no pacote.

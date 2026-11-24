# TabWon 1.00.2 — Referência das funções do código-fonte

Referência rápida das funções e classes do `TabWon.py`, na ordem em que aparecem no arquivo. Os números de linha valem para a versão 1.00.2, entregue em 23/11/2026; funções internas (definidas dentro de outras) não estão listadas.

## Visão geral

- **Arquivo único:** `TabWon.py` (Python 3, tkinter). Ponto de entrada no fim do arquivo: cria `tk.Tk()` e `TabWonApp(root)` e, quando a janela fica pronta, avisa o loader (`_avisar_loader`).
- **Dependências:** pandas, numpy, openpyxl, pyarrow, duckdb (Consultor SQL), matplotlib (gráficos), Pillow (logo/imagens), pywin32 opcional (área de transferência).
- **Pastas:** `TabWon_configs/` da pasta do TabWon (INI, paletas, .lng, ajuda, ícones, cache `.cache_dbc/` e `TabWonDBC.exe` para conversão rápida) e `TabWon_configs/` da pasta do TabWin (consultas salvas, histórico SQL, `.usuarios/` e `.backups_consultas/`). Quando a pasta do TabWin não é configurada, as duas são a mesma.
- **Pasta base:** `PASTA_SCRIPT` é a pasta do `TabWon.py` ou, no executável (PyInstaller), a pasta do `TabWon.exe`. DEFs, CNVs e `TabWon_configs` ficam sempre do lado de fora; dentro do .exe vão só o código e o `TabWon_icos.ico` (usado se o de fora faltar).
- **Pasta do TabWin:** `PASTA_TABWIN` é a pasta dos DEFs e CNVs, gravada em `[pastas] tabwin` do `TabWon_configs.ini` (vazia = pasta do TabWon). Nela, em `PASTA_DADOS` (`<pasta do TabWin>/TabWon_configs`), ficam as consultas salvas, as últimas pesquisas, os baixados e as preferências de cada usuário, compartilhados por todos que usam a mesma pasta. `_definir_pasta_tabwin` troca esses caminhos; na primeira abertura, `_pedir_pasta_tabwin` pergunta a pasta se não houver DEFs.
- **Loader:** na versão compilada, o `TabWon.exe` é o loader em C# (pasta `loader`), que mostra a tela de carregamento e abre o `TabWon_tabulador.exe` (gerado pelo PyInstaller). O tabulador avisa o loader por um evento do Windows cujo nome vem na variável `TABWON_LOADER_EVENTO`. Detalhes em `loader/README.md`.
- **Abertura rápida:** matplotlib (`_carregar_mpl`) e DuckDB (`_DuckDBSobDemanda`) só são importados no primeiro uso.
- **Ganchos no Tk:** o programa substitui `_options`, `_configure`, `cget`, `BaseWidget.__init__`, `title` e métodos de Treeview, Notebook, Combobox, Canvas e Listbox. A cadeia é **tradução → ícones → tema**: o texto passa pela tradução, depois pela troca de emoji/palavra por ícone (que consulta o texto original em português guardado em `_textos_orig`) e por fim pela troca de cores. Para acrescentar um gancho novo, siga o mesmo padrão: guarde a função anterior numa variável global e chame-a no fim.
- **Superlog:** cada ajuste do resultado vira um *ato* (`Ato=nome | chave=valor`). `_registrar_ajuste` grava o ato; `_aplicar_ato_silencioso` o reproduz chamando o `_ato_exec_*` correspondente.
- **Sem comentários no código:** por padrão do projeto, a documentação fica fora do arquivo (este documento e a Ajuda).

## Utilitários básicos

| Linha | Função | Descrição |
|---:|---|---|
| 42 | `_carregar_mpl()` | Carrega o matplotlib só na primeira vez que um gráfico é desenhado (abertura mais rápida). |
| 59 | `class _DuckDBSobDemanda` | Substituto do módulo duckdb que só o importa no primeiro uso (Consultor SQL e consultas Q). |
| 60 | `_DuckDBSobDemanda.__getattr__(nome)` | Importa o duckdb de verdade, troca a variável global e devolve o atributo pedido. |
| 129 | `_log(msg)` | Registra uma mensagem com data/hora no registro de eventos da sessão (usado no relatório de problema). |
| 136 | `_normalizar_texto(texto)` | Remove acentos, deixa minúsculo e apara espaços; base de todas as comparações de texto. |
| 142 | `_prefixo_alinhado(prefixo, widget)` | Completa um prefixo (S ▶, L ▶, Q ▶) com espaços até a mesma largura em pixels, para alinhar listas. |
| 160 | `centralizar(janela, parent)` | Centraliza uma janela sobre a janela-mãe (ou na tela) e a traz para frente. |
| 180 | `_bind_escape(janela)` | Faz a tecla Esc fechar a janela. |

## Motor de tema (cores claro/escuro)

| Linha | Função | Descrição |
|---:|---|---|
| 301 | `_para_hex(v)` | Converte qualquer cor do Tk (nome ou #rgb) em #rrggbb. |
| 319 | `_cor_heuristica(tipo, h)` | Calcula a cor equivalente no tema atual quando a cor não está na paleta (clareia/escurece por tipo de uso). |
| 333 | `_traduzir_cor(tipo, v)` | Troca uma cor 'de fábrica' pela cor correspondente da paleta/modo ativos. |
| 344 | `_tipo_opcao(k)` | Classifica uma opção de widget (bg, fg, select, highlight...) para saber como recolorir. |
| 351 | `_traduzir_dict(d)` | Aplica _traduzir_cor a todas as opções de cor de um dicionário de configuração. |
| 367 | `_cores_de(d)` | Extrai apenas as opções de cor de um dicionário de configuração. |
| 383 | `_options_tema(cnf, kw)` | Gancho no _options do Tk: traduz as cores antes de criar/configurar qualquer widget. |
| 391 | `_configure_tema(cmd, cnf, kw)` | Gancho no configure: guarda as cores originais pedidas e aplica as do tema. |
| 408 | `_opcoes_cor_widget(w)` | Lista as opções de cor que um widget aceita. |
| 417 | `_cor_bruta(w, opcao, valor)` | Lê o valor de uma opção diretamente no Tcl, sem passar pelos ganchos. |
| 422 | `_tematizar_widget(w, recolorir)` | Recolore um widget segundo o tema atual, guardando as cores originais. |
| 433 | `_bw_init_tema(master, widgetName, cnf, kw, extra)` | Gancho na criação de widgets: registra cores originais e tematiza na hora. |
| 442 | `_tv_tag_tema(tagname, option, **kw)` | Gancho em Treeview.tag_configure: traduz as cores das tags (zebra, destaques). |
| 450 | `_create_text_tema(*args, **kw)` | Gancho em Canvas.create_text: traduz a cor do texto. |
| 463 | `_canvas_create_tema(itemType, args, kw)` | Gancho na criação de itens de Canvas: registra e traduz cores. |
| 474 | `_canvas_delete_tema(*args)` | Gancho em Canvas.delete: limpa o registro de cores dos itens apagados. |
| 486 | `_lb_delete_tema(first, last)` | Gancho em Listbox.delete: limpa as cores registradas por item. |

## Multilíngue (i18n)

| Linha | Função | Descrição |
|---:|---|---|
| 2092 | `_i18n_desescapar(v)` | Converte \n, \t e aspas escapadas de um texto do .lng. |
| 2103 | `_ler_lng(caminho)` | Lê um arquivo .lng e devolve o dicionário CHAVE → texto. |
| 2128 | `_nome_idioma(caminho)` | Lê o nome exibido do idioma (Idioma_nome) no cabeçalho de um .lng. |
| 2142 | `_codigo_idioma(arquivo)` | Extrai o código (PTBR, ENUS...) do nome do arquivo .lng. |
| 2147 | `_i18n_definir(arquivo)` | Ativa um idioma: monta a tabela de tradução a partir do .lng e dos textos embutidos. |
| 2176 | `_i18n_nucleo(s)` | Traduz um texto exato ou um modelo com variáveis ({1}, {2}). |
| 2196 | `_i18n_um(s)` | Traduz um texto preservando prefixos (emoji, espaços) e sufixos. |
| 2211 | `_t(s)` | Função principal de tradução; aceita textos com várias linhas e usa cache. |
| 2228 | `_i18n_livre(w)` | Indica se um widget está marcado para não ser traduzido (ex.: tabelas de dados). |
| 2249 | `_cb_get_i18n()` | Gancho em Combobox.get: devolve o valor original (em português) do item escolhido. |
| 2255 | `_cb_set_i18n(value)` | Gancho em Combobox.set: mostra o texto traduzido guardando o original. |
| 2263 | `_cb_valores_i18n(d)` | Traduz a lista de valores de um Combobox mantendo o mapa para os originais. |
| 2279 | `_options_i18n(cnf, kw)` | Gancho no _options: traduz text, values e afins antes de chegar ao Tk. |
| 2300 | `_cget_i18n(key)` | Gancho no cget: devolve o texto original quando o widget mostra a tradução. |
| 2310 | `_wm_title_i18n(string)` | Gancho no title() das janelas: traduz o título. |
| 2320 | `_tv_heading_i18n(column, option, **kw)` | Gancho em Treeview.heading: traduz títulos de colunas. |
| 2327 | `_tv_insert_i18n(parent, index, iid, **kw)` | Gancho em Treeview.insert: traduz o texto dos itens (exceto em tabelas de dados). |
| 2338 | `_tv_item_i18n(item, option, **kw)` | Gancho em Treeview.item: traduz o texto ao alterar um item. |
| 2349 | `_nb_reg(child, kw)` | Registra o texto original de uma aba de Notebook e o traduz. |
| 2358 | `_nb_add_i18n(child, **kw)` | Gancho em Notebook.add. |
| 2362 | `_nb_insert_i18n(pos, child, **kw)` | Gancho em Notebook.insert. |
| 2366 | `_nb_tab_i18n(tab_id, option, **kw)` | Gancho em Notebook.tab. |
| 2386 | `_retraduzir(w)` | Percorre a árvore de widgets e reaplica a tradução (troca de idioma sem reiniciar). |
| 2418 | `_idiomas_disponiveis(cfg)` | Lista os idiomas liberados no INI cujos arquivos existem. |
| 2428 | `_idioma_efetivo(cfg, prefs)` | Decide o idioma do usuário (preferência pessoal ou padrão da instalação). |
| 2439 | `_arquivo_por_idioma(caminho)` | Escolhe a versão do arquivo no idioma ativo (ex.: TabWon_ajuda_ENUS.txt). |

## Ícones PNG, Baixados e Últimas pesquisas

| Linha | Função | Descrição |
|---:|---|---|
| 2503 | `_norm_txt_ic(v)` | Normaliza o texto de um botão para procurar o ícone pelo texto (Fechar, Cancelar...). |
| 2509 | `_icone_espaco()` | Cria a imagem transparente de 1 px que iguala a altura de botões sem ícone. |
| 2521 | `_icone_fixo(w, nome)` | Força um ícone específico em um widget (ex.: tipos de gráfico). |
| 2528 | `_eh_simbolo_ic(tok)` | Diz se um trecho de texto é um símbolo/emoji (candidato a virar ícone PNG). |
| 2537 | `_modelos_icones_achados()` | Lista os modelos de ícones existentes na pasta icones (pelos *_claro.png). |
| 2545 | `_modelos_icones_disponiveis(cfg)` | Lista os modelos liberados no INI que existem na pasta. |
| 2553 | `_modelo_icones_efetivo(cfg, prefs)` | Decide o modelo de ícones do usuário (preferência, padrão da instalação ou Emoji). |
| 2564 | `_definir_modelo_icones(modelo)` | Ativa um modelo de ícones (vazio = emoji). |
| 2568 | `_folha_icones(modelo, escuro)` | Carrega (com cache) a folha PNG do modelo para o modo claro ou escuro. |
| 2581 | `_icone(nome, secao, modelo, escuro)` | Recorta e devolve um ícone de 24 px da folha (parte A, B ou C), com cache. |
| 2607 | `_icone_alerta(tipo)` | Devolve o ícone de 48 px de informação, aviso, erro ou pergunta. |
| 2629 | `_secao_cor(w, cor)` | Escolhe a parte da folha pela cor de fundo: A (neutro claro), B (escuro) ou C (colorido claro). |
| 2639 | `_secao_widget(w)` | Aplica _secao_cor à cor de fundo de um widget. |
| 2644 | `_icone_em_botao(btn, nome, texto)` | Transforma um botão em botão só com ícone (ou volta ao texto no modo Emoji). |
| 2666 | `_ic_aplicar_imagem(w)` | Coloca/retira a imagem automática de um widget, ajustando a largura em pixels. |
| 2704 | `_ic_cnf(d)` | Decide o ícone automático pelo texto (emoji no início/fim, seta sozinha, palavra como Fechar) ou espaçador. |
| 2761 | `_ic_menu(d)` | Troca o emoji dos itens de menu por ícone PNG. |
| 2775 | `_options_icones(cnf, kw)` | Gancho no _options que aplica a troca automática de emoji por ícone. |
| 2792 | `_bw_init_ic(*a, **k)` | Gancho na criação de widgets que aplica a imagem automática. |
| 2797 | `_configure_ic(cmd, cnf, kw)` | Gancho no configure que atualiza o ícone quando muda texto ou cor de fundo. |
| 2810 | `_cget_ic(key)` | Gancho no cget que devolve o texto original (com emoji) em vez do exibido. |
| 2830 | `_wm_title_ic(string)` | Gancho no title() que tira o emoji dos títulos das janelas quando há modelo de ícones. |
| 2840 | `_reaplicar_icones(raiz)` | Percorre as janelas e reaplica ícones (troca de modelo ou de modo de cor). |
| 2861 | `_vivo(w)` | Verifica se um widget ainda existe. |
| 2866 | `_arquivo_baixados()` | Caminho do JSON de arquivos baixados do usuário. |
| 2870 | `_ler_baixados()` | Lê a lista de arquivos salvos pelo TabWon. |
| 2878 | `_gravar_baixados(lista)` | Grava a lista de baixados (máximo 40). |
| 2887 | `_arquivo_recentes()` | Caminho do JSON de últimas pesquisas do usuário. |
| 2891 | `_ler_recentes()` | Lê as últimas pesquisas. |
| 2899 | `_gravar_recentes(lista)` | Grava as últimas pesquisas (máximo 10). |

## Paletas, tema e estilos ttk

| Linha | Função | Descrição |
|---:|---|---|
| 2908 | `_caminho_paleta(arquivo)` | Resolve o caminho de um arquivo de paleta de cores. |
| 2915 | `_ler_paleta(arquivo)` | Lê uma paleta (.ini de cores) com cache por data de modificação. |
| 2947 | `_sistema_escuro()` | Consulta o registro do Windows para saber se o sistema está em modo escuro. |
| 2959 | `_definir_tema(arquivo, modo)` | Ativa paleta e modo (claro, escuro, automático) no dicionário global de tema. |
| 2970 | `_retematizar(w)` | Recolore toda a árvore de widgets com o tema atual. |
| 3016 | `_aplicar_estilos_ttk(root)` | Configura os estilos ttk (Treeview, Combobox, Notebook, Progressbar) com as cores do tema. |

## INI, consultas salvas e preferências do usuário

| Linha | Função | Descrição |
|---:|---|---|
| 3070 | `_ocultar_pasta(caminho)` | Marca uma pasta como oculta no Windows. |
| 3080 | `_ini_definir(texto, secao, chave, valor, comentario)` | Altera/insere uma chave numa seção do texto do INI, preservando comentários. |
| 3113 | `class _TravaConsultas` | Trava de arquivo para gravação simultânea do arquivo de consultas salvas. |
| 3114 | `_TravaConsultas.__init__(caminho, espera)` | Guarda o caminho da trava e o tempo máximo de espera. |
| 3117 | `_TravaConsultas.__enter__()` | Espera e cria o arquivo de trava (com PID e hora). |
| 3137 | `_TravaConsultas.__exit__(*a)` | Remove a trava. |
| 3147 | `_backup_consultas(forcar, manter)` | Copia o arquivo de consultas salvas para .backups_consultas (mantém as 40 últimas). |
| 3166 | `_ler_blocos_consultas()` | Lê o arquivo de consultas salvas e separa em blocos. |
| 3172 | `_anexar_consultas(texto)` | Acrescenta consultas ao arquivo com trava e backup. |
| 3224 | `_pasta_downloads()` | Pasta Downloads do usuário (pelo Windows; se não houver, a pasta do usuário). |
| 3240 | `_formas_caminho(caminho)` | Formas normalizadas de um caminho (absoluto e real), para comparar unidades mapeadas e caminhos de rede. |
| 3248 | `_caminho_dentro(caminho, pasta)` | Diz se um caminho está dentro de uma pasta ou de uma subpasta dela. |
| 3255 | `_usuario_atual()` | Nome do usuário do Windows, limpo para nome de arquivo. |
| 3264 | `_arquivo_prefs()` | Caminho do INI de preferências do usuário. |
| 3267 | `_caminho_pasta_tabwin(valor)` | Converte o valor do INI no caminho da pasta do TabWin (vazio = pasta do TabWon; caminho relativo = a partir da pasta do TabWon). |
| 3274 | `_definir_pasta_tabwin(valor)` | Ativa a pasta do TabWin: troca `PASTA_TABWIN`, `PASTA_DADOS` e os caminhos das consultas salvas, das preferências, das cópias de segurança e do histórico SQL. |
| 3289 | `_contar_defs(pasta)` | Conta os arquivos DEF de uma pasta (-1 se não der para ler). |
| 3294 | `_pasta_gravavel(pasta)` | Testa se é possível gravar numa pasta (cria e apaga um arquivo de teste). |
| 3305 | `_gravar_pasta_tabwin_ini(valor)` | Grava a pasta do TabWin na seção [pastas] do TabWon_configs.ini. |
| 3316 | `_ler_config_ini()` | Lê o TabWon_configs.ini (menu, consultas, organização, cores, idioma, ícones e pasta do TabWin), com padrões e migrações. |
| 3497 | `_saudacao_por_horario(agora)` | Bom dia / Boa tarde / Boa noite. |
| 3505 | `_limpar_nome_usuario(nome)` | Limpa o nome digitado (uma linha, espaços simples, até 50 caracteres). |
| 3509 | `_ler_prefs_usuario()` | Lê as preferências do usuário (nome, paleta, modo, idioma, ícones, zoom...). |
| 3540 | `_gravar_prefs_usuario(prefs)` | Grava as preferências do usuário. |
| 3567 | `_excluir_prefs_usuario()` | Apaga o arquivo de preferências do usuário. |

## Dicas (tooltip) e diálogos

| Linha | Função | Descrição |
|---:|---|---|
| 3572 | `class Tooltip` | Dica flutuante exibida ao passar o mouse. |
| 3573 | `Tooltip.__init__(widget, texto, delay)` | Liga a dica a um widget. |
| 3581 | `Tooltip._enter(event)` | Agenda a exibição. |
| 3585 | `Tooltip._leave(event)` | Cancela/oculta. |
| 3587 | `Tooltip._cancel()` | Cancela o agendamento. |
| 3592 | `Tooltip._show()` | Cria a janelinha da dica posicionada perto do mouse, dentro da tela. |
| 3626 | `Tooltip._hide()` | Fecha a dica. |
| 3633 | `_adicionar_tooltip(widget, texto)` | Atalho para criar um Tooltip. |
| 3645 | `_janela_mae(parent)` | Descobre a janela que deve ser dona de um diálogo (a que tem foco/grab). |
| 3663 | `_dialogo(titulo, mensagem, botoes, icone, parent, padrao, cancelar, detalhe)` | Diálogo próprio do TabWon (substitui messagebox), com ícone PNG, botões e detalhe. |
| 3749 | `_mb_substituto(nome)` | Cria as versões de showinfo/askyesno etc. que usam _dialogo. |

## Datas e formatação de valores

| Linha | Função | Descrição |
|---:|---|---|
| 3794 | `parse_data_qualquer(txt)` | Converte texto em data aceitando vários formatos (dd/mm/aaaa, aaaa-mm-dd, aaaamm...). |
| 3817 | `formatar_br(valor)` | Formata número no padrão brasileiro (1.234,5). |
| 3827 | `_formatar_numero(num, casas, milhar)` | Formata número com casas decimais e separador de milhar. |
| 3840 | `_formatar_valor(valor, formato)` | Formata um valor conforme o formato da coluna (número, moeda, %, data, texto). |
| 3879 | `_openpyxl_format(fmt)` | Converte o formato de coluna do TabWon em formato numérico do Excel. |
| 3904 | `_descricao_formato_legivel(fmt)` | Descreve um formato em texto (ex.: 'Moeda, 2 casas'). |
| 3925 | `carregar_imagem_com_fundo(img_path, max_h, bg_hex)` | Carrega uma imagem (logo) redimensionada e mesclada com a cor de fundo. |

## Cache, DBC e CNV

| Linha | Função | Descrição |
|---:|---|---|
| 3948 | `_carregar_index()` | Lê o índice do cache de DBC. |
| 3958 | `_salvar_index(index)` | Grava o índice do cache de forma atômica. |
| 3974 | `_hash_rapido(caminho, chunk)` | Hash MD5 rápido (primeiro e último 1 MB) para identificar se um DBC mudou. |
| 3985 | `_limpar_orfaos()` | Apaga arquivos de cache que não estão mais no índice. |
| 4027 | `_codificacao_texto(caminho)` | Descobre a codificação de um DEF ou CNV: UTF-8 (com ou sem BOM) ou latin-1. |
| 4042 | `_limite_desc_cnv(cab)` | Tamanho máximo da descrição no CNV: 100 no formato novo (cabeçalho com N) e 50 no formato clássico. |
| 4046 | `_coluna_codigos_cnv(linhas, largo)` | Coluna onde começam os códigos nas linhas do CNV (60 no formato clássico; detectada no formato novo). |
| 4059 | `_resolver_caminho(caminho, pasta_def)` | Encontra um arquivo citado no DEF (caminho absoluto, relativo ou só o nome). |
| 4075 | `_pasta_ci(pasta)` | Acha uma pasta sem diferenciar maiúsculas e minúsculas (útil em servidores de rede). |
| 4092 | `_arquivos_na_pasta(pasta, mascara)` | Lista os arquivos de uma pasta que batem com a máscara (* e ?), sem diferenciar maiúsculas e minúsculas. |
| 4110 | `_listar_arquivos_def(spec, pasta_def)` | Resolve a linha A do DEF: usa a pasta escrita no DEF e, só se ela não existir, procura na pasta do DEF e na pasta do TabWin. Não repete arquivos vistos por caminhos diferentes. |
| 4135 | `_dividir_spec(spec)` | Separa 'arquivo.cnv,posição' em caminho e posição. |
| 4140 | `_cnv_existe(spec)` | Verifica se o CNV de uma especificação existe. |
| 4145 | `_ler_dbf_tabela(caminho)` | Lê uma tabela DBF simples (usada como de-para). |
| 4165 | `_campos_do_dbc(caminho)` | Lê os nomes e tamanhos dos campos de um DBC sem descompactar os dados. |
| 4185 | `_expandir_codigo(p, n_chars, curto)` | Expande códigos e faixas do CNV (ex.: 01-05, A00-A09). |
| 4209 | `ler_cnv(spec)` | Lê e interpreta um CNV (ou DBF de-para) com cache: categorias, códigos, subtotais. Aceita o formato clássico e o novo do TabWin (cabeçalho com N, descrições de até 100 caracteres, UTF-8). Linhas com o mesmo número formam uma categoria com a descrição da última linha. |
| 4338 | `parse_cnv(caminho_cnv)` | Atalho para ler_cnv devolvendo o mapa código → descrição. |
| 4344 | `_cnv_itens(spec)` | Lista as descrições do CNV na ordem do arquivo. |
| 4350 | `_cnv_rotulos_dos_codigos(spec, codigos)` | Descrições do CNV correspondentes a uma lista de códigos. |
| 4363 | `_normalizar_codigos(serie, n_chars, pos)` | Ajusta códigos de uma coluna ao tamanho do CNV (zeros à esquerda, recorte). |
| 4376 | `aplicar_cnv(serie, caminho_cnv, pos)` | Troca os códigos de uma coluna pelas descrições do CNV (rápido, vetorizado). Nas faixas, códigos mais curtos são completados com espaços, como no TabWin. |
| 4426 | `_rotulos_validos_cnv(cnv)` | Conjunto das descrições válidas de um CNV. |
| 4437 | `_tratar_nao_classificados(serie, item, modo)` | Aplica a regra de não classificados (ignorar, incluir ou discriminar). |
| 4454 | `_resolver_descricoes_cnv(descs, spec)` | Converte descrições escolhidas em filtros nos códigos correspondentes. |
| 4502 | `_converter_serie(serie, item)` | Aplica CNV ou recorte a uma coluna conforme o item do DEF. |
| 4509 | `_somar_sem_subtotais(df, subtotais)` | Soma uma coluna ignorando as linhas de subtotal. |
| 4521 | `_aplicar_subtotais(pivot, cnv)` | Insere as linhas de subtotal definidas no CNV. |

## Análise e edição de DEF/CNV

| Linha | Função | Descrição |
|---:|---|---|
| 4544 | `_arquivos_da_linha_a(padrao, pasta_def)` | Expande o padrão da linha A do DEF (com * e ?) na lista de DBCs (usa `_listar_arquivos_def`). |
| 4548 | `_validar_cnv_arquivo(caminho)` | Confere se um CNV pode ser lido e aponta o primeiro problema. |
| 4576 | `_analisar_def(texto, pasta_def)` | Analisa um DEF: arquivos, campos, CNVs, linhas repetidas; devolve problemas e sugestões. |
| 4688 | `_aplicar_unificacoes_def(texto, sugestoes)` | Une linhas L/C/S repetidas em uma linha X (sugestão do Analisar DEF). |
| 4705 | `_ler_cnv_para_edicao(caminho)` | Lê um CNV em estrutura editável (cabeçalho, formato, codificação e linhas) para o editor de CNV. |
| 4763 | `_montar_texto_cnv(cab, linhas, ncat)` | Monta o texto do CNV a partir do cabeçalho e das linhas editadas, no mesmo formato do original (clássico ou novo). |
| 4794 | `_validar_linhas_cnv(cab, linhas)` | Valida as linhas do CNV (códigos repetidos, tamanhos, sequenciais). |
| 4842 | `_aplicar_correcoes_def(texto, correcoes)` | Aplica no texto do DEF as correções aceitas na análise. |
| 4865 | `_problemas_linhas_cnv(cab, linhas)` | Lista os problemas de cada linha do CNV para destacar no editor. |
| 4917 | `_reunir_rotulos(pecas, rotulos)` | Reconstrói descrições que contêm vírgula quando vêm partidas em pedaços. |

## Consultor SQL e conversão log ⟺ SQL ⟺ DEF

| Linha | Função | Descrição |
|---:|---|---|
| 4952 | `_sql_sem_textos(sql, manter_identificadores)` | Retira strings e comentários do SQL para analisá-lo com segurança. |
| 4976 | `_validar_sql_leitura(sql)` | Garante que o SQL é só leitura (um único SELECT/WITH). |
| 4996 | `_sql_sem_ponto_virgula(sql)` | Tira o ponto e vírgula final do SQL. |
| 5002 | `_nome_tabela_dbc(caminho, existentes)` | Cria um nome de tabela SQL válido e único a partir do nome do DBC. |
| 5012 | `_sql_ident(nome)` | Põe um identificador entre aspas duplas. |
| 5016 | `_sql_texto(v)` | Põe um texto entre aspas simples, escapando. |
| 5020 | `_sql_tabelas_usadas(sql, tabelas)` | Descobre quais tabelas (DBCs) o SQL usa. |
| 5029 | `_sql_tipo_duck(nome)` | Converte um nome de tipo em tipo do DuckDB. |
| 5034 | `_udf_cnv(valores, specs, posicoes)` | Função SQL que troca códigos por descrições de um CNV. |
| 5050 | `_udf_cnv_ordem(rotulos, specs)` | Função SQL que devolve a posição da descrição no CNV (ordem do CNV). |
| 5062 | `_sql_preparar_conexao(tabelas, cancelado, ao_converter)` | Abre o DuckDB, converte os DBCs para cache e registra tabelas e funções CNV. |
| 5091 | `_sql_valor(v, numerico)` | Formata um valor para o SQL (número ou texto). |
| 5101 | `_sql_lista_valores(valor)` | Separa uma lista de valores digitada. |
| 5106 | `_sql_coluna_expr(col)` | Monta a expressão SQL de uma coluna (campo, CNV, agregação, apelido). |
| 5119 | `_sql_coluna_valor(col)` | Expressão do valor de uma coluna (com ou sem CNV). |
| 5128 | `_sql_condicao(c, tipos)` | Monta uma condição WHERE (=, entre, contém, lista, faixa...). |
| 5159 | `_sql_de_modelo(modelo)` | Gera o SQL completo a partir dos passos do Consultor (arquivos, colunas, condições, ordem). |
| 5207 | `_ler_def_itens(caminho)` | Lê um DEF e devolve linhas, colunas, incrementos, seleções e DBCs. |
| 5245 | `_resolver_def_do_log(def_valor)` | Acha o DEF citado num log (caminho original ou pasta do TabWin). |
| 5252 | `_modelo_de_log(texto)` | Converte um log/consulta salva em passos do Consultor SQL. |
| 5319 | `_titulo_def_seguro(t)` | Limpa um título para usar em DEF. |
| 5324 | `_numero_br(v)` | Número em texto brasileiro, sem casas desnecessárias. |
| 5332 | `_faixas_para_condicao(c, dec)` | Converte faixas numéricas em condições para DEF/CNV. |
| 5357 | `_texto_cnv_faixas(titulo, cats)` | Gera o texto de um CNV de faixas. |
| 5365 | `_plano_def_de_modelo(modelo)` | Planeja um DEF (linhas e CNVs) a partir do que foi montado no Consultor. |
| 5442 | `_texto_def_do_plano(plano, caminhos_cnv)` | Gera o texto do DEF a partir do plano, substituindo os caminhos dos CNVs. |
| 5451 | `_log_de_modelo(modelo, def_path, selecoes_plano)` | Gera o log TabWon equivalente ao modelo do Consultor ('Virar consulta salva'). |
| 5504 | `_ler_sql_salvos()` | Lê o histórico de SQL. |
| 5520 | `_gravar_sql_salvos(itens)` | Grava o histórico de SQL. |
| 5537 | `_texto_consulta_sql(sql, tabelas, modelo)` | Monta o texto de uma consulta salva do tipo SQL (Q ▶). |
| 5545 | `_ler_consulta_sql(texto)` | Lê uma consulta salva do tipo SQL (SQL, tabelas e modelo). |

## Nomes de consultas salvas

| Linha | Função | Descrição |
|---:|---|---|
| 5564 | `_pedir_nome(pai, titulo, rotulo, inicial)` | Pequena janela para digitar um nome. |
| 5590 | `_montar_nome_consulta(ident, nome)` | Junta identificador e nome ('Equipe A - Nome'). |
| 5597 | `_separar_nome_consulta(completo)` | Separa identificador e nome de uma consulta salva. |
| 5605 | `_campos_nome_consulta(pai, inicial, fonte)` | Campos de identificador + nome usados nas janelas de salvar/renomear. |
| 5637 | `_pedir_nome_consulta(pai, titulo, inicial)` | Janela para escolher identificador e nome de uma consulta. |

## Distribuições e cálculos estatísticos

| Linha | Função | Descrição |
|---:|---|---|
| 5661 | `_betacf(a, b, x)` | Fração contínua da função beta incompleta (cálculo estatístico interno). |
| 5684 | `_betainc(a, b, x)` | Função beta incompleta regularizada. |
| 5693 | `_gammainc_q(a, x)` | Função gama incompleta superior regularizada. |
| 5714 | `_t_p_bicaudal(t, gl)` | Valor-p bicaudal da distribuição t. |
| 5719 | `_t_cdf(t, gl)` | Distribuição acumulada t. |
| 5724 | `_t_ppf(p, gl)` | Quantil da distribuição t (para intervalos de confiança). |
| 5735 | `_f_sf(f, d1, d2)` | Cauda superior da distribuição F (valor-p da regressão). |
| 5740 | `_chi2_sf(x, k)` | Cauda superior do qui-quadrado. |
| 5744 | `_beta_ppf(p, a, b)` | Quantil da distribuição beta (intervalo de credibilidade bayesiano). |
| 5754 | `_fmt_p(p)` | Formata um valor-p. |
| 5760 | `_fmt_n(v, casas)` | Formata um número para os relatórios estatísticos. |
| 5819 | `_calc_num(df, c)` | Converte uma coluna em número (texto inválido vira vazio). |
| 5823 | `_calcular_coluna(chave, df_sem, cols, params)` | Calcula as Funções (sequência, acumular, normalizar, indicador, ranking, variação...). |
| 5881 | `_total_calculo(formula, cols, tot, df_sem, destino)` | Calcula o TOTAL de uma coluna criada por função (soma, recálculo do indicador etc.). |
| 5970 | `_matriz_num(df, cols)` | Matriz numérica das colunas escolhidas. |
| 5976 | `_estat_linhas(tipo, M, params)` | Calcula as estatísticas por linha (médias, mediana, moda, quartis, desvio, IC...). |
| 6070 | `_priori_beta(a, b, modo)` | Define a priori beta da análise bayesiana. |
| 6085 | `_validar_estat(tipo, n_cols, params)` | Verifica se a estatística escolhida tem colunas e parâmetros suficientes. |
| 6112 | `_relatorio_correlacao(df, cols, metodo)` | Relatório de correlação de Pearson ou Spearman. |
| 6142 | `_regressao(df, cols)` | Regressão linear (mínimos quadrados) entre colunas. |
| 6167 | `_relatorio_regressao(df, cols)` | Relatório da regressão (coeficientes, R², valor-p). |
| 6192 | `_relatorio_teste(df, cols, params)` | Relatório dos testes t (pareado, Welch, uma amostra) e qui-quadrado. |
| 6263 | `_relatorio_bayes(df, cols, params, alfa, beta, saidas)` | Relatório da análise bayesiana de taxas. |

## Auxiliares de gráficos e superlog

| Linha | Função | Descrição |
|---:|---|---|
| 6289 | `_abreviar(s, n)` | Encurta um texto com reticências. |
| 6294 | `_rotular_x(ax, labels, limite, max_rotulos)` | Coloca rótulos no eixo X evitando excesso. |
| 6302 | `_rotular_y(ax, labels, limite, max_rotulos)` | Coloca rótulos no eixo Y evitando excesso. |
| 6310 | `_folga_rotulos(a)` | Abre espaço para rótulos e valores dentro do gráfico. |
| 6345 | `_estilizar_eixos(fig)` | Aplica o estilo padrão de eixos aos gráficos. |
| 6357 | `_legenda_topo(fig, ax, titulo, mostrar)` | Posiciona a legenda no topo do gráfico. |
| 6386 | `_unir_atos(*listas)` | Junta listas de ajustes (superlog) sem duplicar o trecho comum. |

## Cache Parquet, compressão DCL e conversor DBF ⟺ DBC

| Linha | Função | Descrição |
|---:|---|---|
| 6414 | `class _TravaArquivo` | Trava genérica de arquivo (cache de DBC) com tempo de vencimento. |
| 6415 | `_TravaArquivo.__init__(caminho, espera, vencida)` | Guarda caminho, espera e vencimento. |
| 6419 | `_TravaArquivo.tentar()` | Tenta criar a trava; remove travas vencidas. |
| 6437 | `_TravaArquivo.liberar()` | Remove a trava. |
| 6443 | `_TravaArquivo.__enter__()` | Espera até conseguir a trava. |
| 6451 | `_TravaArquivo.__exit__(*a)` | Libera a trava. |
| 6455 | `_info_cache(dbc_path)` | Dados que identificam um DBC no cache (tamanho, data, hash). |
| 6470 | `_cache_valido(dbc_path)` | Devolve o Parquet em cache do DBC que este usuário consegue ler: primeiro o da rede (`.cache_dbc`), depois o cache local; `None` se não houver. |
| 6463 | `_legivel(caminho)` | Diz se o arquivo pode ser aberto para leitura pelo usuário atual (detecta "Acesso negado"). |
| 6479 | `_criar_pasta_lote()` | Cria a pasta temporária de conversão herdando as permissões da pasta do cache (na rede ou, se não der, no cache local). |
| 6491 | `_guardar_local(parquet_gerado, info)` | Guarda o Parquet convertido no cache local do usuário, quando o cache da rede não pode ser usado. |
| 6504 | `_registrar_no_index(dbc_path, info)` | Registra um DBC convertido no índice do cache. |
| 6518 | `_guardar_no_cache(dbc_path, parquet_gerado, info)` | Move o Parquet convertido para o cache da rede e registra no índice; se a rede recusar a gravação ou a leitura, guarda no cache local. |
| 6545 | `_colunas_existentes(parquet_path, colunas)` | Filtra as colunas pedidas que existem no Parquet. |
| 6557 | `_ler_parquet(caminho, colunas)` | Lê só as colunas necessárias de um Parquet. |
| 6563 | `_rodar_conversor(args, cancelado, timeout)` | Executa o TabWonDBC.exe com tempo limite e cancelamento. |
| 6581 | `_converter_lotes(dbcs, infos, cancelado)` | Converte vários DBCs para Parquet em paralelo (lotes por núcleo). |
| 6618 | `_converter_para_cache(dbcs, cancelado, ao_iniciar)` | Garante que os DBCs estejam no cache, convertendo os que faltam. |
| 6657 | `_ler_dbc_com_cache(dbc_path, colunas)` | Lê um DBC (via cache Parquet; sem cache, usa leitura direta). |
| 6692 | `_dcl_comprimentos(rep)` | Expande a tabela de comprimentos do algoritmo PKWARE DCL (compressão DBC). |
| 6699 | `_dcl_codigos(rep)` | Gera os códigos Huffman do DCL. |
| 6716 | `class _LeitorBits` | Lê bits de um buffer (descompressão DCL). |
| 6719 | `_LeitorBits.__init__(dados, pos)` | Inicia o leitor. |
| 6722 | `_LeitorBits.bits(k)` | Lê k bits. |
| 6729 | `_LeitorBits.decod(tabela)` | Decodifica um símbolo Huffman. |
| 6738 | `_dcl_explodir(dados, inicio, ao_progresso)` | Descompacta dados DCL (DBC → DBF em Python). |
| 6764 | `class _EscritorBits` | Escreve bits num buffer (compressão DCL). |
| 6767 | `_EscritorBits.__init__()` | Inicia o escritor. |
| 6770 | `_EscritorBits.put(v, k)` | Escreve k bits. |
| 6775 | `_EscritorBits.huff(code, L)` | Escreve um código Huffman. |
| 6781 | `_EscritorBits.fim()` | Fecha o buffer. |
| 6786 | `_dcl_tabelas_codificacao()` | Tabelas de codificação do DCL. |
| 6804 | `_dcl_implodir(dados, ao_progresso)` | Compacta dados com DCL (DBF → DBC em Python). |
| 6853 | `_cabecalho_dbase(caminho)` | Lê o cabeçalho de um DBF (registros, tamanho do cabeçalho e do registro). |
| 6862 | `_versao_arquivo_exe(caminho)` | Lê a versão de um .exe no Windows. |
| 6890 | `_info_conversor()` | Descobre o TabWonDBC.exe, sua versão e recursos. |
| 6912 | `_descricao_conversor()` | Texto 'Motor: TabWonDBC x.y' exibido no conversor. |
| 6922 | `_conversor_tem_dbf()` | Diz se o conversor sabe converter DBF ⟺ DBC. |
| 6927 | `_conversor_tem_teste()` | Diz se o conversor sabe testar CRC. |
| 6932 | `_testar_dbc(caminho, cancelado, ao_progresso)` | Testa a integridade de um DBC (CRC e tamanho). |
| 6982 | `_converter_dbf_dbc_python(origem, destino, para, ao_progresso)` | Conversão DBF ⟺ DBC em Python puro (alternativa lenta). |
| 7010 | `_converter_dbf_dbc(origem, destino, para, cancelado, ao_progresso)` | Converte DBF ⟺ DBC usando o TabWonDBC.exe (ou Python). |
| 7049 | `_dividir_caminho_a(c)` | Separa pasta e padrão de nome da linha A do DEF. |
| 7056 | `_regras_pastas_defs()` | Lê as pastas e padrões de nome de todos os DEFs da pasta do TabWin (para o conversor). |
| 7090 | `_caminho_local(pasta)` | Normaliza o caminho de uma pasta. |
| 7095 | `_prefixos_para(regras, ext)` | Prefixos de nome esperados para uma extensão numa pasta. |
| 7100 | `_nome_valido(stem)` | Verifica se o nome do arquivo tem só caracteres aceitos. |
| 7105 | `_nome_conforme(stem, prefixos)` | Verifica se o nome segue o padrão da pasta (arquivos em laranja no conversor). |
| 7113 | `_tam_legivel(n)` | Tamanho de arquivo legível (KB, MB, GB). |

## Tipos de coluna, datas e calendário

| Linha | Função | Descrição |
|---:|---|---|
| 7125 | `_letra_coluna(n)` | Letra de coluna estilo Excel (A, B, ..., AA). |
| 7133 | `_tipo_coluna_serie(serie)` | Detecta se uma coluna é número, data ou texto. |
| 7156 | `_inferir_tipos_df(df)` | Detecta os tipos de todas as colunas. |
| 7160 | `_serie_para_data(s)` | Converte uma coluna em datas. |
| 7168 | `_converter_valor_num(v)` | Converte texto (formato brasileiro ou não) em número. |
| 7180 | `_validar_valores_para_tipo(valores, tipo)` | Confere se os valores servem para o tipo escolhido. |
| 7196 | `_aplicar_shift_data(d, qtd, unidade)` | Soma dias, meses ou anos a uma data. |
| 7212 | `_formatar_hhmmss(segundos)` | Segundos em hh:mm:ss. |
| 7217 | `_hex_para_rgb(hex_cor)` | Cor #rrggbb em tupla RGB. |
| 7222 | `abrir_calendario(parent, entry, formato)` | Calendário para escolher uma data num campo. |

## Painéis reutilizáveis

| Linha | Função | Descrição |
|---:|---|---|
| 7267 | `class FormatadorFrame` | Painel reutilizável para escolher formato de coluna (número, moeda, %, data, texto, autoincremento). |
| 7268 | `FormatadorFrame.__init__(master, fmt_inicial, titulo, tipos_permitidos, permitir_porcento, permitir_modo, modo_inicial, travar_porcentagem, travar_data)` | Monta os controles conforme os tipos permitidos. |
| 7348 | `FormatadorFrame._validar_numero_digitado(proposto)` | Aceita só números no campo de casas/valor. |
| 7352 | `FormatadorFrame._carregar(fmt)` | Preenche os controles com um formato existente. |
| 7372 | `FormatadorFrame._limpar_widgets_l1()` | Esconde os controles da primeira linha. |
| 7377 | `FormatadorFrame._atualizar_habilitacao_autoinc()` | Habilita/desabilita as opções de autoincremento. |
| 7388 | `FormatadorFrame._atualizar()` | Mostra os controles adequados ao tipo escolhido. |
| 7438 | `FormatadorFrame.get_formato()` | Devolve o formato escolhido como dicionário (e valida). |
| 7482 | `class ProgressPanel` | Tela de progresso da tabulação. |
| 7493 | `ProgressPanel.__init__(parent_frame, root, on_cancelar)` | Cobre a tela com logo, barra animada, frases, tempo e Cancelar. |
| 7535 | `ProgressPanel.atualizar_tempo()` | Atualiza o tempo decorrido a cada segundo. |
| 7541 | `ProgressPanel.atualizar_frase()` | Troca a frase de status periodicamente. |
| 7550 | `ProgressPanel.cancelar_imediato()` | Cancela o processamento. |
| 7556 | `ProgressPanel.close()` | Fecha a tela de progresso. |

## Leitura de log, superlog e treemap

| Linha | Função | Descrição |
|---:|---|---|
| 7563 | `_round_rect(canvas, x1, y1, x2, y2, r, **kwargs)` | Desenha retângulo arredondado num Canvas. |
| 7572 | `_parse_log_tabwin(texto, nomes_selecoes)` | Lê um log (TabWon ou TabWin): DEF, opções, seleções, arquivos e ajustes. |
| 7649 | `_parse_kv(valor)` | Lê os parâmetros chave=valor de um ato do superlog. |
| 7663 | `_parse_ato(linha)` | Separa um ato do superlog em nome e parâmetros. |
| 7765 | `_treemap_retangulos(valores, x, y, w, h)` | Calcula os retângulos do treemap (algoritmo squarified). |

## TabWonApp — inicialização, desfazer e superlog

| Linha | Função | Descrição |
|---:|---|---|
| 7806 | `class TabWonApp` | Classe principal: monta a janela, guarda o estado da consulta e implementa todas as telas. |
| 7807 | `TabWonApp.__init__(root)` | Lê INI e preferências, ativa tema, idioma e ícones, cria a interface, atalhos e estado inicial. |
| 7944 | `TabWonApp._atualizar_visual_dbcs()` | Mostra no título de Arquivos quando a escolha de DBCs está travada pelo log. |
| 7960 | `TabWonApp._travar_listbox_dbcs(lb)` | Bloqueia cliques na lista de DBCs quando a quantidade é exigida pelo log. |
| 7971 | `TabWonApp._dbcs_todos()` | Seleciona todos os DBCs. |
| 7977 | `TabWonApp._dbcs_limpar()` | Desmarca todos os DBCs. |
| 7983 | `TabWonApp._qtd_dbcs_tabulados()` | Quantos arquivos o log atual usou. |
| 7997 | `TabWonApp._confirmar_fechamento()` | Pergunta antes de fechar o programa. |
| 8024 | `TabWonApp._ref_coluna(nome)` | Referência de uma coluna do resultado (letra estilo Excel). |
| 8031 | `TabWonApp._rotulo_coluna(nome)` | Rótulo 'A: Nome' de uma coluna. |
| 8034 | `TabWonApp._snapshot_estado()` | Fotografa o estado do resultado (tabela, formatos, gráficos, ajustes) para desfazer. |
| 8061 | `TabWonApp._registrar_snapshot_base()` | Empilha uma fotografia no histórico (até 60). |
| 8073 | `TabWonApp._aplicar_snapshot(snap)` | Restaura uma fotografia do histórico. |
| 8113 | `TabWonApp._atualizar_botoes_undo_redo()` | Habilita/desabilita desfazer e refazer. |
| 8131 | `TabWonApp._desfazer_base()` | Desfazer (implementação base). |
| 8136 | `TabWonApp._refazer_base()` | Refazer (implementação base). |
| 8141 | `TabWonApp._registrar_ajuste(texto_ato, gravar_snapshot)` | Registra um ato no superlog e tira a fotografia para desfazer. |
| 8152 | `TabWonApp._atualizar_titulo_export()` | Atualiza o título exibido acima da tabela. |
| 8163 | `TabWonApp._atualizar_visibilidade_super_log()` | Mostra o botão Copiar Super Log só quando há ajustes ou gráficos. |
| 8173 | `TabWonApp._ato_criar_coluna(nome, tipo, valor, modo, fmt)` | Monta o texto do ato 'criar coluna'. |
| 8177 | `TabWonApp._ato_formatar_coluna(nome, fmt)` | Monta o ato 'formatar coluna'. |
| 8181 | `TabWonApp._ato_renomear_coluna(de, para)` | Monta o ato 'renomear coluna'. |
| 8184 | `TabWonApp._ato_mover_coluna(nome, posicao_1based)` | Monta o ato 'mover coluna'. |
| 8187 | `TabWonApp._ato_excluir_coluna(nome)` | Monta o ato 'excluir coluna'. |
| 8190 | `TabWonApp._ato_excluir_linha(idx_1based, rotulo)` | Monta o ato 'excluir linha'. |
| 8198 | `TabWonApp._ato_ordenar(nome, asc)` | Monta o ato 'ordenar'. |
| 8201 | `TabWonApp._ato_operacao(tipo, colunas, valor_extra, destino, fmt, acao_data, formula)` | Monta o ato de uma operação (+ − × ÷ % datas). |
| 8223 | `TabWonApp._ato_set_detalhes(chave, valor)` | Monta o ato de detalhes (título, cor, TOTAL...). |
| 8226 | `TabWonApp._ato_grafico_criar(cfg)` | Monta o ato 'criar gráfico'. |
| 8229 | `TabWonApp._ato_grafico_excluir(gid)` | Monta o ato 'excluir gráfico'. |
| 8232 | `TabWonApp._aplicar_ato_silencioso(linha_ato)` | Executa um ato do superlog sem abrir janelas (despacha para _ato_exec_*). |
| 8277 | `TabWonApp._ato_exec_grafico_criar(cfg)` | Recria um gráfico a partir do ato. |
| 8293 | `TabWonApp._ato_exec_grafico_excluir(kw)` | Exclui um gráfico a partir do ato. |
| 8299 | `TabWonApp._ato_exec_set_detalhes(kw)` | Aplica detalhes (título, cor, zebra, TOTAL...). |
| 8321 | `TabWonApp._ato_exec_renomear_primeira_coluna(kw)` | Renomeia a primeira coluna (categorias). |
| 8327 | `TabWonApp._ato_exec_mostrar_total(kw)` | Mostra/oculta a linha TOTAL. |
| 8341 | `TabWonApp._colunas_base_total()` | Colunas da tabulação (originais) que entram na soma da coluna Total. |
| 8347 | `TabWonApp._recalcular_coluna_total()` | Recalcula a coluna Total (soma de cada linha) e a recoloca depois das colunas da tabulação, se estiver visível. |
| 8363 | `TabWonApp._ato_exec_mostrar_coluna_total(kw)` | Mostra ou oculta a coluna Total (ato do superlog). |
| 8373 | `TabWonApp._alternar_coluna_total(mostrar)` | Mostra/oculta a coluna Total pelo menu de contexto e registra no superlog. |
| 8380 | `TabWonApp._recalcular_total()` | Recalcula a linha TOTAL (somas, fórmulas de colunas calculadas, tipos compatíveis). |
| 8439 | `TabWonApp._cols_da_formula(formula, df_sem)` | Colunas usadas numa fórmula de coluna calculada. |
| 8460 | `TabWonApp._calcular_total_formula(formula, destino_col, df_sem, totais_parciais)` | Calcula o TOTAL de uma coluna calculada pela fórmula (ex.: % recalculada). |
| 8523 | `TabWonApp._ato_exec_criar_coluna(kw)` | Cria coluna com valor fixo ou autoincremento. |
| 8570 | `TabWonApp._ato_exec_formatar_coluna(kw)` | Aplica formato a uma coluna (convertendo valores se preciso). |
| 8588 | `TabWonApp._ato_exec_renomear_coluna(kw)` | Renomeia coluna. |
| 8599 | `TabWonApp._ato_exec_mover_coluna(kw)` | Move coluna para outra posição. |
| 8612 | `TabWonApp._ato_exec_excluir_coluna(kw)` | Exclui coluna. |
| 8623 | `TabWonApp._ato_exec_excluir_linha(kw)` | Exclui linha (por posição/rótulo). |
| 8641 | `TabWonApp._ato_exec_excluir_total(kw)` | Exclui a linha TOTAL. |
| 8646 | `TabWonApp._ato_exec_ordenar(kw)` | Ordena a tabela por uma coluna, mantendo o TOTAL no fim. |
| 8666 | `TabWonApp._resolver_lista_colunas_por_nome_ou_pos(colunas)` | Converte referências de colunas (nome ou posição) nos nomes atuais. |
| 8690 | `TabWonApp._calcular_operacao_pura(tipo, colunas, valor_extra, fmt, acao_data)` | Calcula o resultado de uma operação entre colunas. |
| 8740 | `TabWonApp._ato_exec_operacao(kw)` | Cria a coluna de uma operação. |
| 8792 | `TabWonApp._aplicar_ajustes_silencioso(atos)` | Aplica uma lista de atos (reprodução do superlog). |

## TabWonApp — janela e painel de gráficos

| Linha | Função | Descrição |
|---:|---|---|
| 8799 | `TabWonApp._calcular_largura_painel()` | Largura do painel de gráficos. |
| 8802 | `TabWonApp._abrir_painel_graficos()` | Mostra o painel de gráficos ao lado da tabela. |
| 8825 | `TabWonApp._fechar_painel_graficos()` | Recolhe o painel de gráficos. |
| 8837 | `TabWonApp._reflow_barra(forcar)` | Reorganiza a barra de ferramentas do resultado conforme a largura da janela. |
| 8866 | `TabWonApp._atualizar_btn_ver_graficos()` | Mostra/esconde o botão Ver gráficos. |
| 8897 | `TabWonApp._atualizar_visibilidade_painel_graficos()` | Abre/fecha o painel conforme existam gráficos. |
| 8912 | `TabWonApp._habilitar_scroll_graficos()` | Liga a rolagem do mouse no painel de gráficos. |
| 8949 | `TabWonApp._on_window_configure(event)` | Reage ao redimensionamento/troca de monitor da janela. |
| 8971 | `TabWonApp._aplicar_restauracao_85()` | Ao sair de maximizado, restaura a janela em 85% da tela (ou no tamanho mínimo se arrastando). |
| 8984 | `TabWonApp._ajustar_ao_trocar_tela()` | Reajusta o tamanho ao mover a janela para outro monitor. |
| 8995 | `TabWonApp._monitor_da_janela()` | Identifica o monitor onde a janela está (Windows). |
| 9004 | `TabWonApp._botao_mouse_pressionado()` | Diz se o botão esquerdo do mouse está pressionado (Windows). |

## TabWonApp — editores, Consultor SQL e conversor

| Linha | Função | Descrição |
|---:|---|---|
| 9012 | `TabWonApp._atualizar_lista_defs()` | Recarrega a lista de DEFs (pasta do TabWin, em ordem alfabética). |
| 9020 | `TabWonApp.abrir_editor_def(texto_inicial, ao_salvar, pai)` | Editor de DEF completo (abrir, novo, gerar de DBC, inserir linha, analisar, salvar). |
| 9505 | `TabWonApp.abrir_editor_cnv(caminho, ao_fechar, pai)` | Editor de CNV completo (linhas, validação, correção guiada, gerar de DBC/DBF). |
| 10086 | `TabWonApp.abrir_consultor_sql(consulta)` | Consultor SQL passo a passo (início, arquivos, colunas, condições, ordem, SQL e resultado). |
| 11081 | `TabWonApp.abrir_conversor_dbf_dbc()` | Conversor DBF ⟺ DBC (listas, conversão, CRC, integridade, corrigir local, importar...). |

## TabWonApp — menu, tema, idioma, recentes, baixados e configurações

| Linha | Função | Descrição |
|---:|---|---|
| 11950 | `TabWonApp._abrir_menu_extra(event)` | Monta e abre o menu ⚙ (Shift + clique abre as configurações avançadas). |
| 11977 | `TabWonApp._aplicar_tema(arquivo, modo, salvar)` | Troca paleta/modo, recolore tudo, reaplica ícones e opcionalmente salva. |
| 11997 | `TabWonApp._rerenderizar_tela()` | Redesenha a tela atual após troca de tema. |
| 12021 | `TabWonApp._atualizar_botao_tema()` | Atualiza o ícone de modo de cor (botão não exibido na barra). |
| 12034 | `TabWonApp._alternar_tema_visual()` | Alterna claro → escuro → automático. |
| 12038 | `TabWonApp._vigiar_tema_auto()` | No modo automático, confere o modo do Windows a cada hora. |
| 12049 | `TabWonApp._paletas_disponiveis()` | Paletas liberadas no INI. |
| 12057 | `TabWonApp._aplicar_idioma()` | Ativa o idioma escolhido e retraduz a interface. |
| 12070 | `TabWonApp._montar_idioma_config(corpo, top)` | Bloco de idioma das Configurações do usuário. |
| 12117 | `TabWonApp._montar_aba_idioma(aba_, cfg)` | Aba Idioma das Configurações avançadas (idiomas disponíveis e padrão). |
| 12182 | `TabWonApp.finalizar_sucesso(*a, **k)` | Envolve _finalizar_sucesso_base: cria/reaproveita o registro de Últimas pesquisas. |
| 12195 | `TabWonApp._registrar_snapshot()` | Envolve a fotografia de desfazer e agenda salvar a pesquisa recente. |
| 12200 | `TabWonApp.desfazer()` | Desfazer (Ctrl+Z) e atualizar a pesquisa recente. |
| 12203 | `TabWonApp.refazer()` | Refazer (Ctrl+Y) e atualizar a pesquisa recente. |
| 12206 | `TabWonApp._agendar_recente()` | Agenda (com atraso) a gravação da pesquisa recente. |
| 12214 | `TabWonApp._salvar_recente()` | Grava a pesquisa atual (superlog completo) nas Últimas pesquisas. |
| 12237 | `TabWonApp.abrir_recentes()` | Janela Últimas pesquisas (executar, tirar da lista). |
| 12289 | `TabWonApp._executar_recente(e)` | Carrega o superlog de uma pesquisa recente e executa direto. |
| 12319 | `TabWonApp._registrar_baixado(caminho)` | Acrescenta um arquivo salvo à lista de Baixados. |
| 12329 | `TabWonApp._atualizar_botao_baixados()` | Mostra/esconde os botões Baixados (topo e tela de resultado). |
| 12344 | `TabWonApp.abrir_baixados()` | Janela Baixados (abrir arquivo/pasta, tirar da lista, limpar). |
| 12425 | `TabWonApp._montar_icones_config(f_ap, linha)` | Combobox de modelo de ícones com prévia nas Configurações. |
| 12471 | `TabWonApp.abrir_modal_configuracoes()` | Configurações do usuário (aparência, idioma, nome, excluir preferências). |

## TabWonApp — resultado SQL, Sobre e Ajuda

| Linha | Função | Descrição |
|---:|---|---|
| 12651 | `TabWonApp.executar_consulta_sql(nome, sql, tabelas, modelo)` | Executa uma consulta SQL salva e mostra o resultado na tela de tabulação. |
| 12695 | `TabWonApp._erro_consulta_sql(m)` | Mostra erro de consulta SQL. |
| 12702 | `TabWonApp._finalizar_consulta_sql(df, dt)` | Exibe o resultado de uma consulta SQL. |
| 12729 | `TabWonApp._sair_resultado_sql()` | Sai do modo resultado SQL e restaura a barra superior. |
| 12741 | `TabWonApp._salvar_resultado_sql()` | Exporta o resultado da consulta SQL. |
| 12757 | `TabWonApp._executar_entrada_sql(entry)` | Executa uma consulta salva do tipo Q ▶. |
| 12763 | `TabWonApp._abrir_sobre()` | Abre o Sobre no idioma ativo. |
| 12766 | `TabWonApp._abrir_ajuda()` | Abre a Ajuda no idioma ativo. |
| 12769 | `TabWonApp._abrir_modal_sobre(caminho)` | Janela Sobre (logo e texto do arquivo TabWon_sobre). |
| 12847 | `TabWonApp._abrir_modal_ajuda(caminho)` | Janela de Ajuda com seções, busca e negrito (*texto*). |

## TabWonApp — tela inicial, seleção e log

| Linha | Função | Descrição |
|---:|---|---|
| 13107 | `TabWonApp.setup_ui()` | Cria a estrutura principal da janela (barra superior, área central, rodapé). |
| 13170 | `TabWonApp._criar_botoes_tela_selecao()` | Cria os botões Limpar seleções e Início. |
| 13187 | `TabWonApp._remover_botoes_tela_selecao()` | Remove esses botões. |
| 13195 | `TabWonApp.limpar_selecoes()` | Volta ao DEF puro (zera ajustes, gráficos, seleções e travas). |
| 13261 | `TabWonApp.ir_para_inicio()` | Volta à tela inicial. |
| 13313 | `TabWonApp._texto_saudacao()` | Texto de saudação com o nome do usuário. |
| 13317 | `TabWonApp._atualizar_saudacao()` | Atualiza a saudação. |
| 13323 | `TabWonApp._erro_callback(tipo, valor, tb)` | Captura erros não tratados do Tk e grava no registro. |
| 13329 | `TabWonApp._exportacao_restrita(caminho)` | Diz se um destino de exportação está na pasta do TabWon ou do TabWin (proibido, salvo liberação nas Configurações avançadas). |
| 13335 | `TabWonApp._pasta_exportacao_inicial()` | Pasta em que a janela de salvar abre: a última usada (se permitida) ou Downloads. |
| 13340 | `TabWonApp._avisar_pasta_proibida(pasta, parent)` | Avisa que não é permitido salvar exportações naquela pasta. |
| 13344 | `TabWonApp._pedir_arquivo_exportacao(parent, **kw)` | Janela de salvar XLSX/CSV que recusa as pastas proibidas e lembra a última pasta usada. |
| 13359 | `TabWonApp._motivo_pedir_pasta_tabwin()` | Diz se é preciso pedir a pasta do TabWin: pasta configurada inacessível ou nenhum DEF na pasta do TabWon. |
| 13365 | `TabWonApp._migrar_dados_usuario(pasta_usuarios_ant, log_ant)` | Ao trocar a pasta do TabWin, copia as preferências do usuário e oferece copiar as consultas salvas, se a nova pasta não tiver. |
| 13388 | `TabWonApp._trocar_pasta_tabwin(valor)` | Troca a pasta do TabWin em uso e recarrega DEFs, consultas salvas, preferências, tema, idioma e ícones. |
| 13416 | `TabWonApp._pedir_pasta_tabwin(motivo, ao_concluir)` | Janela "Onde estão os arquivos do TabWin?": procura a pasta, confere os DEFs e a permissão de gravação e grava no INI. |
| 13527 | `TabWonApp._verificar_boas_vindas()` | No primeiro uso, pede a pasta do TabWin (se não houver DEFs na pasta do TabWon) e o nome do usuário. |
| 13599 | `TabWonApp.mostrar_welcome()` | Monta a tela inicial (logo, DEF, consultas salvas, colar log). |
| 13765 | `TabWonApp._criar_widgets_frame_main()` | Cria as listas de Linhas, Colunas, Incremento, Seleções, Arquivos e o log. |
| 13885 | `TabWonApp._travar_listbox(lb)` | Impede alterar uma lista (superlog travado). |
| 13898 | `TabWonApp._travar_dbcs_por_log(quantidade)` | Exige a quantidade de DBCs definida pelo log. |
| 13904 | `TabWonApp._destravar_dbcs()` | Libera a escolha de DBCs. |
| 13911 | `TabWonApp._aplicar_travamento_super_log(travado)` | Trava/destrava as listas quando um superlog está carregado. |
| 13953 | `TabWonApp._aplicar_cores_grupos()` | Aplica as cores de grupo definidas no DEF às listas. |
| 13979 | `TabWonApp._on_paste_log(event)` | Ao colar na caixa de log, aplica automaticamente. |
| 13982 | `TabWonApp._aplicar_log_do_texto()` | Aplica o log digitado/colado na tela inicial. |
| 13994 | `TabWonApp._atualizar_log_preview(force)` | Atualiza o log exibido conforme as escolhas da tela de seleção. |
| 14057 | `TabWonApp._match_incremento(nome_log)` | Acha na lista o incremento citado no log. |
| 14068 | `TabWonApp.aplicar_log(texto)` | Aplica um log/superlog: carrega o DEF, marca opções, seleções, arquivos e guarda os ajustes. |
| 14216 | `TabWonApp._tem_ajustes_no_texto(texto)` | Diz se um texto de log contém [Ajustes]. |
| 14220 | `TabWonApp.carregar_logs_salvos()` | Carrega a lista de consultas salvas no seletor. |
| 14247 | `TabWonApp.on_log_salvo_selected(event)` | Carrega a consulta salva escolhida. |
| 14289 | `TabWonApp._salvar_logs_arquivo()` | Regrava o arquivo de consultas salvas (com trava e backup). |

## TabWonApp — novidades, atalhos, zoom, relatório e lote

| Linha | Função | Descrição |
|---:|---|---|
| 14320 | `TabWonApp.abrir_novidades(automatico)` | Janela Novidades da versão. |
| 14340 | `TabWonApp._ver_novidades_se_preciso()` | Controle de exibição das novidades (hoje só pelo menu). |
| 14347 | `TabWonApp.abrir_atalhos()` | Janela de atalhos de teclado. |
| 14373 | `TabWonApp._na_tela_principal()` | Diz se nenhuma janela modal está aberta. |
| 14380 | `TabWonApp._no_resultado()` | Diz se a tela de resultado está ativa. |
| 14384 | `TabWonApp._atalho(acao)` | Executa a ação de um atalho conforme a tela. |
| 14407 | `TabWonApp._buscar_proximo()` | F3: próximo resultado da busca. |
| 14415 | `TabWonApp._instalar_atalhos()` | Liga os atalhos de teclado. |
| 14428 | `TabWonApp._altura_linha()` | Altura da linha da tabela conforme o zoom. |
| 14431 | `TabWonApp._fonte_tabela()` | Fonte da tabela conforme o zoom. |
| 14434 | `TabWonApp._zoom_tabela(passo)` | Aumenta/diminui/restaura o zoom da tabela e salva. |
| 14443 | `TabWonApp.gerar_relatorio_problema()` | Gera o ZIP de relatório de problema. |
| 14491 | `TabWonApp.abrir_lote()` | Janela Rodar consultas em lote. |
| 14560 | `TabWonApp._iniciar_lote(itens, pasta, abrir)` | Começa a execução em lote. |
| 14592 | `TabWonApp._lote_proxima()` | Executa a próxima consulta do lote. |
| 14623 | `TabWonApp._lote_vigiar()` | Acompanha o fim de cada tabulação do lote e grava o XLSX. |
| 14656 | `TabWonApp._lote_concluiu(ok, msg)` | Registra o resultado de uma consulta do lote. |
| 14660 | `TabWonApp._lote_fim()` | Mostra o resumo do lote. |

## TabWonApp — consultas, DEF, seleções e processamento

| Linha | Função | Descrição |
|---:|---|---|
| 14678 | `TabWonApp.gerenciar_consultas_salvas()` | Janela Gerenciar (carregar, renomear, excluir, editar SQL). |
| 14819 | `TabWonApp.mostrar_campos(mostrar)` | Mostra/esconde a área de seleção. |
| 14830 | `TabWonApp.verificar_pendencias()` | Lista o que falta escolher para executar. |
| 14847 | `TabWonApp._ajustar_modo_incremento()` | Incremento múltiplo com Coluna 'Não ativa'; único com Coluna ativa. |
| 14859 | `TabWonApp.validar_botao_executar()` | Habilita o botão Executar Tabulação quando não há pendências. |
| 14865 | `TabWonApp.carregar_defs()` | Lista os DEFs da pasta do TabWin, em ordem alfabética. |
| 14873 | `TabWonApp.on_def_selected(event)` | Carrega o DEF escolhido e preenche todas as listas. |
| 14995 | `TabWonApp.on_coluna_selected(event)` | Reage à escolha de coluna. |
| 15002 | `TabWonApp.on_incremento_selected(event)` | Reage à escolha de incremento. |
| 15009 | `TabWonApp.atualizar_lista_ativa(idx)` | Atualiza a lista de seleções ativas. |
| 15025 | `TabWonApp.incluir_selecao()` | Move uma seleção para Ativas. |
| 15046 | `TabWonApp.excluir_selecao()` | Tira uma seleção de Ativas. |
| 15068 | `TabWonApp.configurar_filtro_ativo()` | Janela de valores de uma seleção (marcar categorias, buscar, colar códigos). |
| 15220 | `TabWonApp.executar_tabulacao()` | Valida e inicia o processamento em segundo plano com a tela de progresso. |
| 15291 | `TabWonApp._resetar_graficos()` | Apaga os gráficos. |
| 15298 | `TabWonApp._restaurar_estado()` | Repreenche as listas a partir do DEF carregado. |
| 15344 | `TabWonApp._serie_normalizada(df, coluna, n_chars, cache, pos)` | Coluna de códigos normalizada (com cache) para filtrar. |
| 15351 | `TabWonApp._serie_e_conjunto_filtro(df, item, nome_sel, cache_series, cache_codes)` | Coluna e conjunto de códigos aceitos de uma seleção. |
| 15377 | `TabWonApp._codigos_do_filtro(nome_sel, cache_codes)` | Códigos aceitos de uma seleção. |
| 15383 | `TabWonApp._colunas_necessarias(linha_info, coluna_info, incrementos, filtros)` | Lista as colunas do DBC realmente necessárias (leitura mais rápida). |
| 15397 | `TabWonApp._avisar_conversao(qtd)` | Informa que DBCs estão sendo convertidos para o cache. |
| 15407 | `TabWonApp.processar_dados(linha_info, coluna_info, incrementos, dbcs_escolhidos, filtros, ign_linhas, ign_colunas)` | Lê os DBCs, aplica filtros e CNVs e monta a tabela cruzada, com a coluna Total quando há Coluna ativa e mais de uma coluna (roda em thread). |

## TabWonApp — tela de resultado e gráficos

| Linha | Função | Descrição |
|---:|---|---|
| 15574 | `TabWonApp._finalizar_sucesso_base(pivot, regs, tempo, erros, total_original, tempo_seg)` | Mostra a tela de resultado (barras, tabela, log, painel) e reaplica ajustes do superlog. |
| 15885 | `TabWonApp._render_graficos()` | Desenha todos os gráficos no painel. |
| 15919 | `TabWonApp._grafico_tem_incompatibilidade(cfg)` | Verifica se o gráfico aponta para colunas inexistentes ('Necessário editar'). |
| 15950 | `TabWonApp._normalizar_eixos_cfg(cfg)` | Ajusta a configuração de eixos do gráfico às colunas atuais. |
| 15994 | `TabWonApp._render_um_grafico_accordion(cfg)` | Cria o card de um gráfico (cabeçalho, editar, excluir, rodapé). |
| 16093 | `TabWonApp._preencher_rodape(txt_widget, texto)` | Escreve o rodapé do gráfico com negrito. |
| 16105 | `TabWonApp._obter_formato_label(coluna)` | Formato de uma coluna para rótulos do gráfico. |
| 16110 | `TabWonApp._desenhar_grafico_inline(cfg, ax, fig, canvas)` | Desenha os tipos clássicos (barras, pizza, área, linhas, dispersão, empilhadas...). |
| 16477 | `TabWonApp._desenhar_ref_estat(ax, cfg, tipo, serie, labels)` | Desenha média, mediana, faixa de desvio e tendência no gráfico. |
| 16504 | `TabWonApp._copiar_grafico_png_cfg(gid)` | Copia o gráfico como imagem com rodapé. |
| 16583 | `TabWonApp._excluir_grafico_cfg(gid)` | Exclui um gráfico. |
| 16594 | `TabWonApp._editar_grafico_cfg(gid)` | Abre o modal para editar um gráfico. |
| 16602 | `TabWonApp._obter_nomes_x()` | Opções de eixo X. |
| 16608 | `TabWonApp._obter_colunas_numericas()` | Colunas numéricas disponíveis para eixo Y. |
| 16613 | `TabWonApp._sugerir_nome_grafico(ignorar_id)` | Sugere 'Gráfico N' sem repetir. |
| 16625 | `TabWonApp._titulo_grafico_em_uso(titulo, ignorar_id)` | Verifica título de gráfico repetido. |
| 16634 | `TabWonApp.abrir_modal_grafico(cfg_editando)` | Modal Criar/Editar gráfico (tipos, eixos, opções, estatística, rodapé). |
| 17111 | `TabWonApp._dados_gauge(cfg, df_sem, cols_sel)` | Prepara os valores do velocímetro e do linear gauge. |
| 17152 | `TabWonApp._desenhar_grafico_novo(tipo, cfg, df_sem, ax, fig, canvas)` | Desenha os tipos novos (histograma, Pareto, mapa de calor, gauges, boxplot, pirâmide, treemap...). |

## TabWonApp — tabela de resultado

| Linha | Função | Descrição |
|---:|---|---|
| 17439 | `TabWonApp._refresh_log_text_widget()` | Atualiza a caixa de log do resultado. |
| 17449 | `TabWonApp._valor_para_exibir(valor, formato, index_eh_total)` | Texto de uma célula conforme o formato da coluna. |
| 17454 | `TabWonApp._autoajustar_colunas_tree()` | Ajusta a largura das colunas ao conteúdo. |
| 17470 | `TabWonApp._render_resultado()` | Preenche a tabela de resultado (formatos, zebra, destaques, TOTAL). |
| 17570 | `TabWonApp._on_tree_right_click(event)` | Menu de contexto da tabela. |
| 17618 | `TabWonApp._ctx_ocultar_total()` | Oculta/mostra o TOTAL pelo menu de contexto. |
| 17627 | `TabWonApp._ctx_excluir_linha(label_linha)` | Exclui linha pelo menu de contexto. |
| 17643 | `TabWonApp._ctx_excluir_coluna(nome_col)` | Exclui coluna pelo menu de contexto. |
| 17654 | `TabWonApp._ctx_formatar_coluna(nome_col)` | Formata coluna pelo menu de contexto. |
| 17658 | `TabWonApp._on_tree_double_click(event)` | Duplo clique: no separador, autoajusta as colunas; numa célula, abre a seleção de texto. |
| 17669 | `TabWonApp._texto_celula(row_id, col_id)` | Texto exibido numa célula do resultado. |
| 17676 | `TabWonApp._copiar_valor_celula(row_id, col_id)` | Copia o valor de uma célula para a área de transferência. |
| 17681 | `TabWonApp._fechar_selecao_celula(*_)` | Fecha o campo de seleção de texto aberto sobre uma célula. |
| 17688 | `TabWonApp._selecionar_texto_celula(row_id, col_id)` | Abre sobre a célula um campo somente leitura para selecionar e copiar parte do texto, sem editar. |
| 17715 | `TabWonApp._on_heading_press(event)` | Início de clique/arraste no cabeçalho. |
| 17725 | `TabWonApp._on_heading_motion(event)` | Arraste de coluna pelo cabeçalho. |
| 17734 | `TabWonApp._on_heading_release(event)` | Solta: ordena (clique) ou move a coluna (arraste). |
| 17763 | `TabWonApp._redesenhar_letras()` | Atualiza a linha de letras (A, B, C) acima da tabela. |
| 17792 | `TabWonApp._redesenhar_numeros()` | Atualiza a numeração das linhas. |

## TabWonApp — formatação, Funções, transposição e busca

| Linha | Função | Descrição |
|---:|---|---|
| 17814 | `TabWonApp._ordenar_pela_primeira(asc)` | Ordena pela primeira coluna (categorias), em ordem natural (números como números), mantendo o TOTAL no fim. |
| 17829 | `TabWonApp._ordenar_por(coluna)` | Ordena pela coluna clicada (alternando crescente/decrescente), inclusive a primeira coluna. |
| 17858 | `TabWonApp._opcoes_colunas_para(operacao)` | Colunas válidas para uma operação. |
| 17869 | `TabWonApp.abrir_modal_formatar_coluna_simples(col_nome)` | Modal rápido de formatação de uma coluna. |
| 17912 | `TabWonApp.abrir_modal_detalhes()` | Modal Detalhes da tabulação (título, 1ª coluna, linha TOTAL, coluna Total, troca de linhas e colunas, cores). |
| 18032 | `TabWonApp._ato_calculo(calc, colunas, params, destinos, fmt)` | Monta o ato de uma Função. |
| 18039 | `TabWonApp._nome_unico(base, reservados)` | Gera nome de coluna único (acrescenta 2, 3...). |
| 18047 | `TabWonApp._rotulo_primeira()` | Rótulo da primeira coluna. |
| 18051 | `TabWonApp._texto_da_coluna(df_sem, col)` | Valores de uma coluna como texto (inclui a 1ª coluna). |
| 18066 | `TabWonApp._calcular_texto(calc, df_sem, cols, params)` | Funções de texto (esquerda, direita, meio, concatenar). |
| 18089 | `TabWonApp._resolver_cols_calculo(cols)` | Resolve as colunas de uma Função. |
| 18098 | `TabWonApp._destinos_calculo(calc, nome, cols, params)` | Nomes das colunas criadas por uma Função. |
| 18104 | `TabWonApp._aplicar_calculo(calc, cols, params, destinos, fmt)` | Cria as colunas de uma Função. |
| 18127 | `TabWonApp._ato_exec_calculo(kw)` | Executa uma Função a partir do ato. |
| 18146 | `TabWonApp.abrir_modal_calculos()` | Abre as Funções (atalho). |
| 18149 | `TabWonApp.abrir_modal_funcoes()` | Modal Funções (ƒ). |
| 18487 | `TabWonApp._transpor_resultado(ativo)` | Troca linhas por colunas (e volta). |
| 18525 | `TabWonApp._ato_exec_transpor(kw)` | Executa a transposição a partir do ato. |
| 18530 | `TabWonApp._tag_destaque(rotulo)` | Tag de cor de um destaque da busca. |
| 18536 | `TabWonApp._configurar_tags_destaque()` | Configura as cores dos destaques. |
| 18541 | `TabWonApp._atualizar_botoes_destaque()` | Mostra/esconde Limpar destaques e Subir/Descer destacados. |
| 18559 | `TabWonApp.limpar_destaques()` | Remove os destaques. |
| 18564 | `TabWonApp.mover_destacados()` | Leva as linhas destacadas para o topo/fim. |
| 18579 | `TabWonApp._mover_linhas(rotulos, posicao)` | Move linhas para o topo ou o fim. |
| 18591 | `TabWonApp._ato_exec_mover_linhas(kw)` | Executa o ato de mover linhas. |
| 18598 | `TabWonApp._remover_marca_busca()` | Tira a marca da célula encontrada. |
| 18605 | `TabWonApp._marcar_celula(item, coluna_idx, texto)` | Marca a célula encontrada. |
| 18619 | `TabWonApp.abrir_busca()` | Janela Buscar na tabela. |

## TabWonApp — configurações avançadas e Estatística

| Linha | Função | Descrição |
|---:|---|---|
| 18730 | `TabWonApp.abrir_config_avancadas()` | Configurações avançadas (pasta do TabWin, menu, consultas, identificadores, cores, ícones, usuários, idioma). |
| 19035 | `TabWonApp._ato_estatistica(est, colunas, params, destinos, fmt)` | Monta o ato de uma estatística. |
| 19043 | `TabWonApp._calcular_estat_saidas(est, cols_reais, params)` | Calcula as colunas de uma estatística. |
| 19063 | `TabWonApp._nomes_destinos(est, nome, params)` | Nomes das colunas criadas por uma estatística. |
| 19077 | `TabWonApp._aplicar_saidas_estat(est, cols_reais, params, destinos, fmt, saidas)` | Cria as colunas da estatística. |
| 19094 | `TabWonApp._ato_exec_estatistica(kw)` | Executa uma estatística a partir do ato. |
| 19112 | `TabWonApp._mostrar_relatorio_estat(titulo, texto, pai)` | Janela de relatório estatístico com Copiar. |
| 19140 | `TabWonApp.abrir_modal_estatistica()` | Modal Estatística (Ø). |

## TabWonApp — operações e edição de colunas

| Linha | Função | Descrição |
|---:|---|---|
| 19386 | `TabWonApp.abrir_modal_operacao(operacao)` | Modal de operação (+ − × ÷ % datas). |
| 19567 | `TabWonApp.abrir_modal_adicionar_coluna()` | Modal Adicionar coluna. |
| 19694 | `TabWonApp.abrir_modal_editar_colunas()` | Modal Editar/Mover colunas. |
| 19787 | `TabWonApp.abrir_modal_excluir()` | Modal Excluir linhas/colunas. |

## TabWonApp — log, cópia, exportação e nova consulta

| Linha | Função | Descrição |
|---:|---|---|
| 19854 | `TabWonApp._texto_log(incluir_ajustes, incluir_graficos)` | Texto do log com ou sem ajustes/gráficos. |
| 19866 | `TabWonApp._avisar_exigencia_arquivos()` | Avisa quando a consulta exige uma quantidade fixa de arquivos. |
| 19901 | `TabWonApp._texto_log_sem_graficos()` | Log sem a seção de gráficos. |
| 19915 | `TabWonApp._texto_log_com_tag(incluir_ajustes, incluir_graficos)` | Log com a marcação de superlog. |
| 19924 | `TabWonApp._nome_ja_existe(nome, ignorar_idx)` | Verifica nome de consulta repetido. |
| 19930 | `TabWonApp.salvar_log()` | Janela Salvar consulta. |
| 20006 | `TabWonApp.copiar_log()` | Copia o log simples. |
| 20019 | `TabWonApp.copiar_super_log()` | Copia o superlog. |
| 20027 | `TabWonApp.abrir_modal_copiar()` | Menu/modal de cópia da tabela. |
| 20087 | `TabWonApp._texto_puro_vertical()` | Tabela como texto puro vertical (WhatsApp). |
| 20107 | `TabWonApp._copiar_imagem_tabela()` | Gera PNG da tabela + log e copia. |
| 20205 | `TabWonApp._montar_log(regs, tempo, erros)` | Monta o log da tabulação (opções, seleções, arquivos, registros, tempo). |
| 20246 | `TabWonApp._abrir_escolha_campos_registros()` | Janela Escolhe Campos de Salvar registros. |
| 20405 | `TabWonApp._executar_exportacao_registros(caminho, campos, tamanhos, ctx)` | Executa a gravação dos registros com progresso. |
| 20467 | `TabWonApp._filtrar_registros(df, ctx)` | Aplica os filtros da consulta aos registros. |
| 20488 | `TabWonApp._gravar_registros_xlsx(caminho, campos, tamanhos, ctx, progresso, cancelado)` | Grava os registros em XLSX. |
| 20565 | `TabWonApp._sanitizar_nome_planilha(nome)` | Limpa o nome da aba do Excel. |
| 20570 | `TabWonApp._exportar_com_log(caminho, formato)` | Grava XLSX/CSV com tabela, formatos e log. |
| 20643 | `TabWonApp._mostrar_sucesso_export(caminho, abrir_arquivo, abrir_pasta, parent)` | Janela 'Salvo com sucesso' (abrir arquivo/diretório) e registra em Baixados. |
| 20666 | `TabWonApp.exportar_xlsx()` | Exportar XLSX (Ctrl+E). |
| 20683 | `TabWonApp.exportar_csv()` | Exportar CSV (Ctrl+Shift+E). |
| 20700 | `TabWonApp._html_tabela(incluir_log)` | Tabela em HTML (cópia formatada). |
| 20741 | `TabWonApp._html_clipboard(html)` | Coloca HTML na área de transferência do Windows. |
| 20764 | `TabWonApp.nova_consulta()` | Volta à seleção mantendo o superlog e os gráficos. |
| 20867 | `TabWonApp.finalizar_erro(msg)` | Mostra erro do processamento e volta à seleção. |

## Inicialização e loader

| Linha | Função | Descrição |
|---:|---|---|
| 20887 | `_avisar_loader()` | Avisa o loader (TabWon.exe) que a janela está pronta, acionando o evento cujo nome vem em `TABWON_LOADER_EVENTO`. Sem loader, não faz nada. |

_Total: 594 funções, métodos e classes._
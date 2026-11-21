# TabWon 1.00.0 — Referência das funções do código-fonte

Referência rápida das funções e classes do `TabWon.py`, na ordem em que aparecem no arquivo. Os números de linha valem para a versão final entregue em 20/11/2026; funções internas (definidas dentro de outras) não estão listadas.

## Visão geral

- **Arquivo único:** `TabWon.py` (Python 3, tkinter). Ponto de entrada no fim do arquivo: cria `tk.Tk()` e `TabWonApp(root)`.
- **Dependências:** pandas, numpy, openpyxl, pyarrow, duckdb (Consultor SQL), matplotlib (gráficos), Pillow (logo/imagens), pywin32 opcional (área de transferência).
- **Pastas:** `TabWon_configs/` (INI, paletas, .lng, ajuda, ícones, logs, cache `.cache_dbc/`, `.usuarios/`, `.backups_consultas/`) e `TabWonDBC.exe` para conversão rápida.
- **Pasta base:** `PASTA_SCRIPT` é a pasta do `TabWon.py` ou, no executável (PyInstaller), a pasta do `TabWon.exe`. DEFs, CNVs e `TabWon_configs` ficam sempre do lado de fora; dentro do .exe vão só o código e o `TabWon_icos.ico` (usado se o de fora faltar).
- **Abertura rápida:** matplotlib (`_carregar_mpl`) e DuckDB (`_DuckDBSobDemanda`) só são importados no primeiro uso.
- **Ganchos no Tk:** o programa substitui `_options`, `_configure`, `cget`, `BaseWidget.__init__`, `title` e métodos de Treeview, Notebook, Combobox, Canvas e Listbox. A cadeia é **tradução → ícones → tema**: o texto passa pela tradução, depois pela troca de emoji/palavra por ícone (que consulta o texto original em português guardado em `_textos_orig`) e por fim pela troca de cores. Para acrescentar um gancho novo, siga o mesmo padrão: guarde a função anterior numa variável global e chame-a no fim.
- **Superlog:** cada ajuste do resultado vira um *ato* (`Ato=nome | chave=valor`). `_registrar_ajuste` grava o ato; `_aplicar_ato_silencioso` o reproduz chamando o `_ato_exec_*` correspondente.
- **Sem comentários no código:** por padrão do projeto, a documentação fica fora do arquivo (este documento e a Ajuda).

## Utilitários básicos

| Linha | Função | Descrição |
|---:|---|---|
| 41 | `_carregar_mpl()` | Carrega o matplotlib só na primeira vez que um gráfico é desenhado (abertura mais rápida). |
| 58 | `class _DuckDBSobDemanda` | Substituto do módulo duckdb que só o importa no primeiro uso (Consultor SQL e consultas Q). |
| 59 | `_DuckDBSobDemanda.__getattr__(nome)` | Importa o duckdb de verdade, troca a variável global e devolve o atributo pedido. |
| 124 | `_log(msg)` | Registra uma mensagem com data/hora no registro de eventos da sessão (usado no relatório de problema). |
| 131 | `_normalizar_texto(texto)` | Remove acentos, deixa minúsculo e apara espaços; base de todas as comparações de texto. |
| 137 | `_prefixo_alinhado(prefixo, widget)` | Completa um prefixo (S ▶, L ▶, Q ▶) com espaços até a mesma largura em pixels, para alinhar listas. |
| 155 | `centralizar(janela, parent)` | Centraliza uma janela sobre a janela-mãe (ou na tela) e a traz para frente. |
| 175 | `_bind_escape(janela)` | Faz a tecla Esc fechar a janela. |

## Motor de tema (cores claro/escuro)

| Linha | Função | Descrição |
|---:|---|---|
| 290 | `_para_hex(v)` | Converte qualquer cor do Tk (nome ou #rgb) em #rrggbb. |
| 308 | `_cor_heuristica(tipo, h)` | Calcula a cor equivalente no tema atual quando a cor não está na paleta (clareia/escurece por tipo de uso). |
| 322 | `_traduzir_cor(tipo, v)` | Troca uma cor 'de fábrica' pela cor correspondente da paleta/modo ativos. |
| 333 | `_tipo_opcao(k)` | Classifica uma opção de widget (bg, fg, select, highlight...) para saber como recolorir. |
| 340 | `_traduzir_dict(d)` | Aplica _traduzir_cor a todas as opções de cor de um dicionário de configuração. |
| 356 | `_cores_de(d)` | Extrai apenas as opções de cor de um dicionário de configuração. |
| 372 | `_options_tema(cnf, kw)` | Gancho no _options do Tk: traduz as cores antes de criar/configurar qualquer widget. |
| 380 | `_configure_tema(cmd, cnf, kw)` | Gancho no configure: guarda as cores originais pedidas e aplica as do tema. |
| 397 | `_opcoes_cor_widget(w)` | Lista as opções de cor que um widget aceita. |
| 406 | `_cor_bruta(w, opcao, valor)` | Lê o valor de uma opção diretamente no Tcl, sem passar pelos ganchos. |
| 411 | `_tematizar_widget(w, recolorir)` | Recolore um widget segundo o tema atual, guardando as cores originais. |
| 422 | `_bw_init_tema(master, widgetName, cnf, kw, extra)` | Gancho na criação de widgets: registra cores originais e tematiza na hora. |
| 431 | `_tv_tag_tema(tagname, option, **kw)` | Gancho em Treeview.tag_configure: traduz as cores das tags (zebra, destaques). |
| 439 | `_create_text_tema(*args, **kw)` | Gancho em Canvas.create_text: traduz a cor do texto. |
| 452 | `_canvas_create_tema(itemType, args, kw)` | Gancho na criação de itens de Canvas: registra e traduz cores. |
| 463 | `_canvas_delete_tema(*args)` | Gancho em Canvas.delete: limpa o registro de cores dos itens apagados. |
| 475 | `_lb_delete_tema(first, last)` | Gancho em Listbox.delete: limpa as cores registradas por item. |

## Multilíngue (i18n)

| Linha | Função | Descrição |
|---:|---|---|
| 2027 | `_i18n_desescapar(v)` | Converte \n, \t e aspas escapadas de um texto do .lng. |
| 2038 | `_ler_lng(caminho)` | Lê um arquivo .lng e devolve o dicionário CHAVE → texto. |
| 2063 | `_nome_idioma(caminho)` | Lê o nome exibido do idioma (Idioma_nome) no cabeçalho de um .lng. |
| 2077 | `_codigo_idioma(arquivo)` | Extrai o código (PTBR, ENUS...) do nome do arquivo .lng. |
| 2082 | `_i18n_definir(arquivo)` | Ativa um idioma: monta a tabela de tradução a partir do .lng e dos textos embutidos. |
| 2111 | `_i18n_nucleo(s)` | Traduz um texto exato ou um modelo com variáveis ({1}, {2}). |
| 2131 | `_i18n_um(s)` | Traduz um texto preservando prefixos (emoji, espaços) e sufixos. |
| 2146 | `_t(s)` | Função principal de tradução; aceita textos com várias linhas e usa cache. |
| 2163 | `_i18n_livre(w)` | Indica se um widget está marcado para não ser traduzido (ex.: tabelas de dados). |
| 2184 | `_cb_get_i18n()` | Gancho em Combobox.get: devolve o valor original (em português) do item escolhido. |
| 2190 | `_cb_set_i18n(value)` | Gancho em Combobox.set: mostra o texto traduzido guardando o original. |
| 2198 | `_cb_valores_i18n(d)` | Traduz a lista de valores de um Combobox mantendo o mapa para os originais. |
| 2214 | `_options_i18n(cnf, kw)` | Gancho no _options: traduz text, values e afins antes de chegar ao Tk. |
| 2235 | `_cget_i18n(key)` | Gancho no cget: devolve o texto original quando o widget mostra a tradução. |
| 2245 | `_wm_title_i18n(string)` | Gancho no title() das janelas: traduz o título. |
| 2255 | `_tv_heading_i18n(column, option, **kw)` | Gancho em Treeview.heading: traduz títulos de colunas. |
| 2262 | `_tv_insert_i18n(parent, index, iid, **kw)` | Gancho em Treeview.insert: traduz o texto dos itens (exceto em tabelas de dados). |
| 2273 | `_tv_item_i18n(item, option, **kw)` | Gancho em Treeview.item: traduz o texto ao alterar um item. |
| 2284 | `_nb_reg(child, kw)` | Registra o texto original de uma aba de Notebook e o traduz. |
| 2293 | `_nb_add_i18n(child, **kw)` | Gancho em Notebook.add. |
| 2297 | `_nb_insert_i18n(pos, child, **kw)` | Gancho em Notebook.insert. |
| 2301 | `_nb_tab_i18n(tab_id, option, **kw)` | Gancho em Notebook.tab. |
| 2321 | `_retraduzir(w)` | Percorre a árvore de widgets e reaplica a tradução (troca de idioma sem reiniciar). |
| 2353 | `_idiomas_disponiveis(cfg)` | Lista os idiomas liberados no INI cujos arquivos existem. |
| 2363 | `_idioma_efetivo(cfg, prefs)` | Decide o idioma do usuário (preferência pessoal ou padrão da instalação). |
| 2374 | `_arquivo_por_idioma(caminho)` | Escolhe a versão do arquivo no idioma ativo (ex.: TabWon_ajuda_ENUS.txt). |

## Ícones PNG, Baixados e Últimas pesquisas

| Linha | Função | Descrição |
|---:|---|---|
| 2438 | `_norm_txt_ic(v)` | Normaliza o texto de um botão para procurar o ícone pelo texto (Fechar, Cancelar...). |
| 2444 | `_icone_espaco()` | Cria a imagem transparente de 1 px que iguala a altura de botões sem ícone. |
| 2456 | `_icone_fixo(w, nome)` | Força um ícone específico em um widget (ex.: tipos de gráfico). |
| 2463 | `_eh_simbolo_ic(tok)` | Diz se um trecho de texto é um símbolo/emoji (candidato a virar ícone PNG). |
| 2472 | `_modelos_icones_achados()` | Lista os modelos de ícones existentes na pasta icones (pelos *_claro.png). |
| 2480 | `_modelos_icones_disponiveis(cfg)` | Lista os modelos liberados no INI que existem na pasta. |
| 2488 | `_modelo_icones_efetivo(cfg, prefs)` | Decide o modelo de ícones do usuário (preferência, padrão da instalação ou Emoji). |
| 2499 | `_definir_modelo_icones(modelo)` | Ativa um modelo de ícones (vazio = emoji). |
| 2503 | `_folha_icones(modelo, escuro)` | Carrega (com cache) a folha PNG do modelo para o modo claro ou escuro. |
| 2516 | `_icone(nome, secao, modelo, escuro)` | Recorta e devolve um ícone de 24 px da folha (parte A, B ou C), com cache. |
| 2542 | `_icone_alerta(tipo)` | Devolve o ícone de 48 px de informação, aviso, erro ou pergunta. |
| 2564 | `_secao_cor(w, cor)` | Escolhe a parte da folha pela cor de fundo: A (neutro claro), B (escuro) ou C (colorido claro). |
| 2574 | `_secao_widget(w)` | Aplica _secao_cor à cor de fundo de um widget. |
| 2579 | `_icone_em_botao(btn, nome, texto)` | Transforma um botão em botão só com ícone (ou volta ao texto no modo Emoji). |
| 2601 | `_ic_aplicar_imagem(w)` | Coloca/retira a imagem automática de um widget, ajustando a largura em pixels. |
| 2639 | `_ic_cnf(d)` | Decide o ícone automático pelo texto (emoji no início/fim, seta sozinha, palavra como Fechar) ou espaçador. |
| 2696 | `_ic_menu(d)` | Troca o emoji dos itens de menu por ícone PNG. |
| 2710 | `_options_icones(cnf, kw)` | Gancho no _options que aplica a troca automática de emoji por ícone. |
| 2727 | `_bw_init_ic(*a, **k)` | Gancho na criação de widgets que aplica a imagem automática. |
| 2732 | `_configure_ic(cmd, cnf, kw)` | Gancho no configure que atualiza o ícone quando muda texto ou cor de fundo. |
| 2745 | `_cget_ic(key)` | Gancho no cget que devolve o texto original (com emoji) em vez do exibido. |
| 2765 | `_wm_title_ic(string)` | Gancho no title() que tira o emoji dos títulos das janelas quando há modelo de ícones. |
| 2775 | `_reaplicar_icones(raiz)` | Percorre as janelas e reaplica ícones (troca de modelo ou de modo de cor). |
| 2796 | `_vivo(w)` | Verifica se um widget ainda existe. |
| 2801 | `_arquivo_baixados()` | Caminho do JSON de arquivos baixados do usuário. |
| 2805 | `_ler_baixados()` | Lê a lista de arquivos salvos pelo TabWon. |
| 2813 | `_gravar_baixados(lista)` | Grava a lista de baixados (máximo 40). |
| 2822 | `_arquivo_recentes()` | Caminho do JSON de últimas pesquisas do usuário. |
| 2826 | `_ler_recentes()` | Lê as últimas pesquisas. |
| 2834 | `_gravar_recentes(lista)` | Grava as últimas pesquisas (máximo 10). |

## Paletas, tema e estilos ttk

| Linha | Função | Descrição |
|---:|---|---|
| 2843 | `_caminho_paleta(arquivo)` | Resolve o caminho de um arquivo de paleta de cores. |
| 2850 | `_ler_paleta(arquivo)` | Lê uma paleta (.ini de cores) com cache por data de modificação. |
| 2882 | `_sistema_escuro()` | Consulta o registro do Windows para saber se o sistema está em modo escuro. |
| 2894 | `_definir_tema(arquivo, modo)` | Ativa paleta e modo (claro, escuro, automático) no dicionário global de tema. |
| 2905 | `_retematizar(w)` | Recolore toda a árvore de widgets com o tema atual. |
| 2951 | `_aplicar_estilos_ttk(root)` | Configura os estilos ttk (Treeview, Combobox, Notebook, Progressbar) com as cores do tema. |

## INI, consultas salvas e preferências do usuário

| Linha | Função | Descrição |
|---:|---|---|
| 3005 | `_ocultar_pasta(caminho)` | Marca uma pasta como oculta no Windows. |
| 3015 | `_ini_definir(texto, secao, chave, valor, comentario)` | Altera/insere uma chave numa seção do texto do INI, preservando comentários. |
| 3048 | `class _TravaConsultas` | Trava de arquivo para gravação simultânea do arquivo de consultas salvas. |
| 3049 | `_TravaConsultas.__init__(caminho, espera)` | Guarda o caminho da trava e o tempo máximo de espera. |
| 3052 | `_TravaConsultas.__enter__()` | Espera e cria o arquivo de trava (com PID e hora). |
| 3072 | `_TravaConsultas.__exit__(*a)` | Remove a trava. |
| 3082 | `_backup_consultas(forcar, manter)` | Copia o arquivo de consultas salvas para .backups_consultas (mantém as 40 últimas). |
| 3101 | `_ler_blocos_consultas()` | Lê o arquivo de consultas salvas e separa em blocos. |
| 3107 | `_anexar_consultas(texto)` | Acrescenta consultas ao arquivo com trava e backup. |
| 3139 | `_usuario_atual()` | Nome do usuário do Windows, limpo para nome de arquivo. |
| 3148 | `_arquivo_prefs()` | Caminho do INI de preferências do usuário. |
| 3152 | `_ler_config_ini()` | Lê o TabWon_configs.ini (menu, consultas, organização, cores, idioma, ícones), com padrões e migrações. |
| 3318 | `_saudacao_por_horario(agora)` | Bom dia / Boa tarde / Boa noite. |
| 3326 | `_limpar_nome_usuario(nome)` | Limpa o nome digitado (uma linha, espaços simples, até 50 caracteres). |
| 3330 | `_ler_prefs_usuario()` | Lê as preferências do usuário (nome, paleta, modo, idioma, ícones, zoom...). |
| 3361 | `_gravar_prefs_usuario(prefs)` | Grava as preferências do usuário. |
| 3386 | `_excluir_prefs_usuario()` | Apaga o arquivo de preferências do usuário. |

## Dicas (tooltip) e diálogos

| Linha | Função | Descrição |
|---:|---|---|
| 3391 | `class Tooltip` | Dica flutuante exibida ao passar o mouse. |
| 3392 | `Tooltip.__init__(widget, texto, delay)` | Liga a dica a um widget. |
| 3400 | `Tooltip._enter(event)` | Agenda a exibição. |
| 3404 | `Tooltip._leave(event)` | Cancela/oculta. |
| 3406 | `Tooltip._cancel()` | Cancela o agendamento. |
| 3411 | `Tooltip._show()` | Cria a janelinha da dica posicionada perto do mouse, dentro da tela. |
| 3445 | `Tooltip._hide()` | Fecha a dica. |
| 3452 | `_adicionar_tooltip(widget, texto)` | Atalho para criar um Tooltip. |
| 3464 | `_janela_mae(parent)` | Descobre a janela que deve ser dona de um diálogo (a que tem foco/grab). |
| 3482 | `_dialogo(titulo, mensagem, botoes, icone, parent, padrao, cancelar, detalhe)` | Diálogo próprio do TabWon (substitui messagebox), com ícone PNG, botões e detalhe. |
| 3568 | `_mb_substituto(nome)` | Cria as versões de showinfo/askyesno etc. que usam _dialogo. |

## Datas e formatação de valores

| Linha | Função | Descrição |
|---:|---|---|
| 3613 | `parse_data_qualquer(txt)` | Converte texto em data aceitando vários formatos (dd/mm/aaaa, aaaa-mm-dd, aaaamm...). |
| 3636 | `formatar_br(valor)` | Formata número no padrão brasileiro (1.234,5). |
| 3646 | `_formatar_numero(num, casas, milhar)` | Formata número com casas decimais e separador de milhar. |
| 3659 | `_formatar_valor(valor, formato)` | Formata um valor conforme o formato da coluna (número, moeda, %, data, texto). |
| 3698 | `_openpyxl_format(fmt)` | Converte o formato de coluna do TabWon em formato numérico do Excel. |
| 3723 | `_descricao_formato_legivel(fmt)` | Descreve um formato em texto (ex.: 'Moeda, 2 casas'). |
| 3744 | `carregar_imagem_com_fundo(img_path, max_h, bg_hex)` | Carrega uma imagem (logo) redimensionada e mesclada com a cor de fundo. |

## Cache, DBC e CNV

| Linha | Função | Descrição |
|---:|---|---|
| 3767 | `_carregar_index()` | Lê o índice do cache de DBC. |
| 3775 | `_salvar_index(index)` | Grava o índice do cache de forma atômica. |
| 3791 | `_hash_rapido(caminho, chunk)` | Hash MD5 rápido (primeiro e último 1 MB) para identificar se um DBC mudou. |
| 3802 | `_limpar_orfaos()` | Apaga arquivos de cache que não estão mais no índice. |
| 3835 | `_resolver_caminho(caminho, pasta_def)` | Encontra um arquivo citado no DEF (caminho absoluto, relativo ou só o nome). |
| 3851 | `_dividir_spec(spec)` | Separa 'arquivo.cnv,posição' em caminho e posição. |
| 3856 | `_cnv_existe(spec)` | Verifica se o CNV de uma especificação existe. |
| 3861 | `_ler_dbf_tabela(caminho)` | Lê uma tabela DBF simples (usada como de-para). |
| 3881 | `_campos_do_dbc(caminho)` | Lê os nomes e tamanhos dos campos de um DBC sem descompactar os dados. |
| 3901 | `_expandir_codigo(p, n_chars, curto)` | Expande códigos e faixas do CNV (ex.: 01-05, A00-A09). |
| 3925 | `ler_cnv(spec)` | Lê e interpreta um CNV (ou DBF de-para) com cache: categorias, códigos, subtotais. |
| 4047 | `parse_cnv(caminho_cnv)` | Atalho para ler_cnv devolvendo o mapa código → descrição. |
| 4053 | `_cnv_itens(spec)` | Lista as descrições do CNV na ordem do arquivo. |
| 4059 | `_cnv_rotulos_dos_codigos(spec, codigos)` | Descrições do CNV correspondentes a uma lista de códigos. |
| 4072 | `_normalizar_codigos(serie, n_chars, pos)` | Ajusta códigos de uma coluna ao tamanho do CNV (zeros à esquerda, recorte). |
| 4085 | `aplicar_cnv(serie, caminho_cnv, pos)` | Troca os códigos de uma coluna pelas descrições do CNV (rápido, vetorizado). |
| 4132 | `_rotulos_validos_cnv(cnv)` | Conjunto das descrições válidas de um CNV. |
| 4143 | `_tratar_nao_classificados(serie, item, modo)` | Aplica a regra de não classificados (ignorar, incluir ou discriminar). |
| 4160 | `_resolver_descricoes_cnv(descs, spec)` | Converte descrições escolhidas em filtros nos códigos correspondentes. |
| 4208 | `_converter_serie(serie, item)` | Aplica CNV ou recorte a uma coluna conforme o item do DEF. |
| 4215 | `_somar_sem_subtotais(df, subtotais)` | Soma uma coluna ignorando as linhas de subtotal. |
| 4227 | `_aplicar_subtotais(pivot, cnv)` | Insere as linhas de subtotal definidas no CNV. |

## Análise e edição de DEF/CNV

| Linha | Função | Descrição |
|---:|---|---|
| 4250 | `_arquivos_da_linha_a(padrao, pasta_def)` | Expande o padrão da linha A do DEF (com * e ?) na lista de DBCs. |
| 4263 | `_validar_cnv_arquivo(caminho)` | Confere se um CNV pode ser lido e aponta o primeiro problema. |
| 4291 | `_analisar_def(texto, pasta_def)` | Analisa um DEF: arquivos, campos, CNVs, linhas repetidas; devolve problemas e sugestões. |
| 4403 | `_aplicar_unificacoes_def(texto, sugestoes)` | Une linhas L/C/S repetidas em uma linha X (sugestão do Analisar DEF). |
| 4420 | `_ler_cnv_para_edicao(caminho)` | Lê um CNV em estrutura editável (cabeçalho e linhas) para o editor de CNV. |
| 4472 | `_montar_texto_cnv(cab, linhas, ncat)` | Monta o texto do CNV a partir do cabeçalho e das linhas editadas. |
| 4499 | `_validar_linhas_cnv(cab, linhas)` | Valida as linhas do CNV (códigos repetidos, tamanhos, sequenciais). |
| 4546 | `_aplicar_correcoes_def(texto, correcoes)` | Aplica no texto do DEF as correções aceitas na análise. |
| 4569 | `_problemas_linhas_cnv(cab, linhas)` | Lista os problemas de cada linha do CNV para destacar no editor. |
| 4620 | `_reunir_rotulos(pecas, rotulos)` | Reconstrói descrições que contêm vírgula quando vêm partidas em pedaços. |

## Consultor SQL e conversão log ⟺ SQL ⟺ DEF

| Linha | Função | Descrição |
|---:|---|---|
| 4654 | `_sql_sem_textos(sql, manter_identificadores)` | Retira strings e comentários do SQL para analisá-lo com segurança. |
| 4678 | `_validar_sql_leitura(sql)` | Garante que o SQL é só leitura (um único SELECT/WITH). |
| 4698 | `_sql_sem_ponto_virgula(sql)` | Tira o ponto e vírgula final do SQL. |
| 4704 | `_nome_tabela_dbc(caminho, existentes)` | Cria um nome de tabela SQL válido e único a partir do nome do DBC. |
| 4714 | `_sql_ident(nome)` | Põe um identificador entre aspas duplas. |
| 4718 | `_sql_texto(v)` | Põe um texto entre aspas simples, escapando. |
| 4722 | `_sql_tabelas_usadas(sql, tabelas)` | Descobre quais tabelas (DBCs) o SQL usa. |
| 4731 | `_sql_tipo_duck(nome)` | Converte um nome de tipo em tipo do DuckDB. |
| 4736 | `_udf_cnv(valores, specs, posicoes)` | Função SQL que troca códigos por descrições de um CNV. |
| 4752 | `_udf_cnv_ordem(rotulos, specs)` | Função SQL que devolve a posição da descrição no CNV (ordem do CNV). |
| 4764 | `_sql_preparar_conexao(tabelas, cancelado, ao_converter)` | Abre o DuckDB, converte os DBCs para cache e registra tabelas e funções CNV. |
| 4793 | `_sql_valor(v, numerico)` | Formata um valor para o SQL (número ou texto). |
| 4803 | `_sql_lista_valores(valor)` | Separa uma lista de valores digitada. |
| 4808 | `_sql_coluna_expr(col)` | Monta a expressão SQL de uma coluna (campo, CNV, agregação, apelido). |
| 4821 | `_sql_coluna_valor(col)` | Expressão do valor de uma coluna (com ou sem CNV). |
| 4830 | `_sql_condicao(c, tipos)` | Monta uma condição WHERE (=, entre, contém, lista, faixa...). |
| 4861 | `_sql_de_modelo(modelo)` | Gera o SQL completo a partir dos passos do Consultor (arquivos, colunas, condições, ordem). |
| 4909 | `_ler_def_itens(caminho)` | Lê um DEF e devolve linhas, colunas, incrementos, seleções e DBCs. |
| 4950 | `_resolver_def_do_log(def_valor)` | Acha o DEF citado num log (caminho original ou pasta do TabWon). |
| 4957 | `_modelo_de_log(texto)` | Converte um log/consulta salva em passos do Consultor SQL. |
| 5024 | `_titulo_def_seguro(t)` | Limpa um título para usar em DEF. |
| 5029 | `_numero_br(v)` | Número em texto brasileiro, sem casas desnecessárias. |
| 5037 | `_faixas_para_condicao(c, dec)` | Converte faixas numéricas em condições para DEF/CNV. |
| 5062 | `_texto_cnv_faixas(titulo, cats)` | Gera o texto de um CNV de faixas. |
| 5070 | `_plano_def_de_modelo(modelo)` | Planeja um DEF (linhas e CNVs) a partir do que foi montado no Consultor. |
| 5147 | `_texto_def_do_plano(plano, caminhos_cnv)` | Gera o texto do DEF a partir do plano, substituindo os caminhos dos CNVs. |
| 5156 | `_log_de_modelo(modelo, def_path, selecoes_plano)` | Gera o log TabWon equivalente ao modelo do Consultor ('Virar consulta salva'). |
| 5209 | `_ler_sql_salvos()` | Lê o histórico de SQL. |
| 5225 | `_gravar_sql_salvos(itens)` | Grava o histórico de SQL. |
| 5236 | `_texto_consulta_sql(sql, tabelas, modelo)` | Monta o texto de uma consulta salva do tipo SQL (Q ▶). |
| 5244 | `_ler_consulta_sql(texto)` | Lê uma consulta salva do tipo SQL (SQL, tabelas e modelo). |

## Nomes de consultas salvas

| Linha | Função | Descrição |
|---:|---|---|
| 5263 | `_pedir_nome(pai, titulo, rotulo, inicial)` | Pequena janela para digitar um nome. |
| 5289 | `_montar_nome_consulta(ident, nome)` | Junta identificador e nome ('Equipe A - Nome'). |
| 5296 | `_separar_nome_consulta(completo)` | Separa identificador e nome de uma consulta salva. |
| 5304 | `_campos_nome_consulta(pai, inicial, fonte)` | Campos de identificador + nome usados nas janelas de salvar/renomear. |
| 5336 | `_pedir_nome_consulta(pai, titulo, inicial)` | Janela para escolher identificador e nome de uma consulta. |

## Distribuições e cálculos estatísticos

| Linha | Função | Descrição |
|---:|---|---|
| 5360 | `_betacf(a, b, x)` | Fração contínua da função beta incompleta (cálculo estatístico interno). |
| 5383 | `_betainc(a, b, x)` | Função beta incompleta regularizada. |
| 5392 | `_gammainc_q(a, x)` | Função gama incompleta superior regularizada. |
| 5413 | `_t_p_bicaudal(t, gl)` | Valor-p bicaudal da distribuição t. |
| 5418 | `_t_cdf(t, gl)` | Distribuição acumulada t. |
| 5423 | `_t_ppf(p, gl)` | Quantil da distribuição t (para intervalos de confiança). |
| 5434 | `_f_sf(f, d1, d2)` | Cauda superior da distribuição F (valor-p da regressão). |
| 5439 | `_chi2_sf(x, k)` | Cauda superior do qui-quadrado. |
| 5443 | `_beta_ppf(p, a, b)` | Quantil da distribuição beta (intervalo de credibilidade bayesiano). |
| 5453 | `_fmt_p(p)` | Formata um valor-p. |
| 5459 | `_fmt_n(v, casas)` | Formata um número para os relatórios estatísticos. |
| 5518 | `_calc_num(df, c)` | Converte uma coluna em número (texto inválido vira vazio). |
| 5522 | `_calcular_coluna(chave, df_sem, cols, params)` | Calcula as Funções (sequência, acumular, normalizar, indicador, ranking, variação...). |
| 5580 | `_total_calculo(formula, cols, tot, df_sem, destino)` | Calcula o TOTAL de uma coluna criada por função (soma, recálculo do indicador etc.). |
| 5669 | `_matriz_num(df, cols)` | Matriz numérica das colunas escolhidas. |
| 5675 | `_estat_linhas(tipo, M, params)` | Calcula as estatísticas por linha (médias, mediana, moda, quartis, desvio, IC...). |
| 5769 | `_priori_beta(a, b, modo)` | Define a priori beta da análise bayesiana. |
| 5784 | `_validar_estat(tipo, n_cols, params)` | Verifica se a estatística escolhida tem colunas e parâmetros suficientes. |
| 5811 | `_relatorio_correlacao(df, cols, metodo)` | Relatório de correlação de Pearson ou Spearman. |
| 5841 | `_regressao(df, cols)` | Regressão linear (mínimos quadrados) entre colunas. |
| 5866 | `_relatorio_regressao(df, cols)` | Relatório da regressão (coeficientes, R², valor-p). |
| 5891 | `_relatorio_teste(df, cols, params)` | Relatório dos testes t (pareado, Welch, uma amostra) e qui-quadrado. |
| 5962 | `_relatorio_bayes(df, cols, params, alfa, beta, saidas)` | Relatório da análise bayesiana de taxas. |

## Auxiliares de gráficos e superlog

| Linha | Função | Descrição |
|---:|---|---|
| 5988 | `_abreviar(s, n)` | Encurta um texto com reticências. |
| 5993 | `_rotular_x(ax, labels, limite, max_rotulos)` | Coloca rótulos no eixo X evitando excesso. |
| 6001 | `_rotular_y(ax, labels, limite, max_rotulos)` | Coloca rótulos no eixo Y evitando excesso. |
| 6009 | `_folga_rotulos(a)` | Abre espaço para rótulos e valores dentro do gráfico. |
| 6044 | `_estilizar_eixos(fig)` | Aplica o estilo padrão de eixos aos gráficos. |
| 6056 | `_legenda_topo(fig, ax, titulo, mostrar)` | Posiciona a legenda no topo do gráfico. |
| 6085 | `_unir_atos(*listas)` | Junta listas de ajustes (superlog) sem duplicar o trecho comum. |

## Cache Parquet, compressão DCL e conversor DBF ⟺ DBC

| Linha | Função | Descrição |
|---:|---|---|
| 6112 | `class _TravaArquivo` | Trava genérica de arquivo (cache de DBC) com tempo de vencimento. |
| 6113 | `_TravaArquivo.__init__(caminho, espera, vencida)` | Guarda caminho, espera e vencimento. |
| 6117 | `_TravaArquivo.tentar()` | Tenta criar a trava; remove travas vencidas. |
| 6135 | `_TravaArquivo.liberar()` | Remove a trava. |
| 6141 | `_TravaArquivo.__enter__()` | Espera até conseguir a trava. |
| 6149 | `_TravaArquivo.__exit__(*a)` | Libera a trava. |
| 6153 | `_info_cache(dbc_path)` | Dados que identificam um DBC no cache (tamanho, data, hash). |
| 6161 | `_cache_valido(dbc_path)` | Verifica se o Parquet em cache ainda corresponde ao DBC. |
| 6168 | `_registrar_no_index(dbc_path, info)` | Registra um DBC convertido no índice do cache. |
| 6180 | `_guardar_no_cache(dbc_path, parquet_gerado, info)` | Move o Parquet gerado para o cache e registra. |
| 6196 | `_colunas_existentes(parquet_path, colunas)` | Filtra as colunas pedidas que existem no Parquet. |
| 6208 | `_ler_parquet(caminho, colunas)` | Lê só as colunas necessárias de um Parquet. |
| 6214 | `_rodar_conversor(args, cancelado, timeout)` | Executa o TabWonDBC.exe com tempo limite e cancelamento. |
| 6232 | `_converter_lotes(dbcs, infos, cancelado)` | Converte vários DBCs para Parquet em paralelo (lotes por núcleo). |
| 6269 | `_converter_para_cache(dbcs, cancelado, ao_iniciar)` | Garante que os DBCs estejam no cache, convertendo os que faltam. |
| 6308 | `_ler_dbc_com_cache(dbc_path, colunas)` | Lê um DBC (via cache Parquet; sem cache, usa leitura direta). |
| 6342 | `_dcl_comprimentos(rep)` | Expande a tabela de comprimentos do algoritmo PKWARE DCL (compressão DBC). |
| 6349 | `_dcl_codigos(rep)` | Gera os códigos Huffman do DCL. |
| 6366 | `class _LeitorBits` | Lê bits de um buffer (descompressão DCL). |
| 6369 | `_LeitorBits.__init__(dados, pos)` | Inicia o leitor. |
| 6372 | `_LeitorBits.bits(k)` | Lê k bits. |
| 6379 | `_LeitorBits.decod(tabela)` | Decodifica um símbolo Huffman. |
| 6388 | `_dcl_explodir(dados, inicio, ao_progresso)` | Descompacta dados DCL (DBC → DBF em Python). |
| 6414 | `class _EscritorBits` | Escreve bits num buffer (compressão DCL). |
| 6417 | `_EscritorBits.__init__()` | Inicia o escritor. |
| 6420 | `_EscritorBits.put(v, k)` | Escreve k bits. |
| 6425 | `_EscritorBits.huff(code, L)` | Escreve um código Huffman. |
| 6431 | `_EscritorBits.fim()` | Fecha o buffer. |
| 6436 | `_dcl_tabelas_codificacao()` | Tabelas de codificação do DCL. |
| 6454 | `_dcl_implodir(dados, ao_progresso)` | Compacta dados com DCL (DBF → DBC em Python). |
| 6503 | `_cabecalho_dbase(caminho)` | Lê o cabeçalho de um DBF (registros, tamanho do cabeçalho e do registro). |
| 6512 | `_versao_arquivo_exe(caminho)` | Lê a versão de um .exe no Windows. |
| 6540 | `_info_conversor()` | Descobre o TabWonDBC.exe, sua versão e recursos. |
| 6562 | `_descricao_conversor()` | Texto 'Motor: TabWonDBC x.y' exibido no conversor. |
| 6572 | `_conversor_tem_dbf()` | Diz se o conversor sabe converter DBF ⟺ DBC. |
| 6577 | `_conversor_tem_teste()` | Diz se o conversor sabe testar CRC. |
| 6582 | `_testar_dbc(caminho, cancelado, ao_progresso)` | Testa a integridade de um DBC (CRC e tamanho). |
| 6632 | `_converter_dbf_dbc_python(origem, destino, para, ao_progresso)` | Conversão DBF ⟺ DBC em Python puro (alternativa lenta). |
| 6660 | `_converter_dbf_dbc(origem, destino, para, cancelado, ao_progresso)` | Converte DBF ⟺ DBC usando o TabWonDBC.exe (ou Python). |
| 6699 | `_dividir_caminho_a(c)` | Separa pasta e padrão de nome da linha A do DEF. |
| 6706 | `_regras_pastas_defs()` | Lê as pastas e padrões de nome de todos os DEFs (para o conversor). |
| 6739 | `_caminho_local(pasta)` | Normaliza o caminho de uma pasta. |
| 6744 | `_prefixos_para(regras, ext)` | Prefixos de nome esperados para uma extensão numa pasta. |
| 6749 | `_nome_valido(stem)` | Verifica se o nome do arquivo tem só caracteres aceitos. |
| 6754 | `_nome_conforme(stem, prefixos)` | Verifica se o nome segue o padrão da pasta (arquivos em laranja no conversor). |
| 6762 | `_tam_legivel(n)` | Tamanho de arquivo legível (KB, MB, GB). |

## Tipos de coluna, datas e calendário

| Linha | Função | Descrição |
|---:|---|---|
| 6774 | `_letra_coluna(n)` | Letra de coluna estilo Excel (A, B, ..., AA). |
| 6782 | `_tipo_coluna_serie(serie)` | Detecta se uma coluna é número, data ou texto. |
| 6805 | `_inferir_tipos_df(df)` | Detecta os tipos de todas as colunas. |
| 6809 | `_serie_para_data(s)` | Converte uma coluna em datas. |
| 6817 | `_converter_valor_num(v)` | Converte texto (formato brasileiro ou não) em número. |
| 6829 | `_validar_valores_para_tipo(valores, tipo)` | Confere se os valores servem para o tipo escolhido. |
| 6845 | `_aplicar_shift_data(d, qtd, unidade)` | Soma dias, meses ou anos a uma data. |
| 6861 | `_formatar_hhmmss(segundos)` | Segundos em hh:mm:ss. |
| 6866 | `_hex_para_rgb(hex_cor)` | Cor #rrggbb em tupla RGB. |
| 6871 | `abrir_calendario(parent, entry, formato)` | Calendário para escolher uma data num campo. |

## Painéis reutilizáveis

| Linha | Função | Descrição |
|---:|---|---|
| 6916 | `class FormatadorFrame` | Painel reutilizável para escolher formato de coluna (número, moeda, %, data, texto, autoincremento). |
| 6917 | `FormatadorFrame.__init__(master, fmt_inicial, titulo, tipos_permitidos, permitir_porcento, permitir_modo, modo_inicial, travar_porcentagem, travar_data)` | Monta os controles conforme os tipos permitidos. |
| 6997 | `FormatadorFrame._validar_numero_digitado(proposto)` | Aceita só números no campo de casas/valor. |
| 7001 | `FormatadorFrame._carregar(fmt)` | Preenche os controles com um formato existente. |
| 7021 | `FormatadorFrame._limpar_widgets_l1()` | Esconde os controles da primeira linha. |
| 7026 | `FormatadorFrame._atualizar_habilitacao_autoinc()` | Habilita/desabilita as opções de autoincremento. |
| 7037 | `FormatadorFrame._atualizar()` | Mostra os controles adequados ao tipo escolhido. |
| 7087 | `FormatadorFrame.get_formato()` | Devolve o formato escolhido como dicionário (e valida). |
| 7131 | `class ProgressPanel` | Tela de progresso da tabulação. |
| 7142 | `ProgressPanel.__init__(parent_frame, root, on_cancelar)` | Cobre a tela com logo, barra animada, frases, tempo e Cancelar. |
| 7184 | `ProgressPanel.atualizar_tempo()` | Atualiza o tempo decorrido a cada segundo. |
| 7190 | `ProgressPanel.atualizar_frase()` | Troca a frase de status periodicamente. |
| 7199 | `ProgressPanel.cancelar_imediato()` | Cancela o processamento. |
| 7205 | `ProgressPanel.close()` | Fecha a tela de progresso. |

## Leitura de log, superlog e treemap

| Linha | Função | Descrição |
|---:|---|---|
| 7212 | `_round_rect(canvas, x1, y1, x2, y2, r, **kwargs)` | Desenha retângulo arredondado num Canvas. |
| 7221 | `_parse_log_tabwin(texto, nomes_selecoes)` | Lê um log (TabWon ou TabWin): DEF, opções, seleções, arquivos e ajustes. |
| 7298 | `_parse_kv(valor)` | Lê os parâmetros chave=valor de um ato do superlog. |
| 7312 | `_parse_ato(linha)` | Separa um ato do superlog em nome e parâmetros. |
| 7414 | `_treemap_retangulos(valores, x, y, w, h)` | Calcula os retângulos do treemap (algoritmo squarified). |

## TabWonApp — inicialização, desfazer e superlog

| Linha | Função | Descrição |
|---:|---|---|
| 7455 | `class TabWonApp` | Classe principal: monta a janela, guarda o estado da consulta e implementa todas as telas. |
| 7456 | `TabWonApp.__init__(root)` | Lê INI e preferências, ativa tema, idioma e ícones, cria a interface, atalhos e estado inicial. |
| 7585 | `TabWonApp._atualizar_visual_dbcs()` | Mostra no título de Arquivos quando a escolha de DBCs está travada pelo log. |
| 7601 | `TabWonApp._travar_listbox_dbcs(lb)` | Bloqueia cliques na lista de DBCs quando a quantidade é exigida pelo log. |
| 7612 | `TabWonApp._dbcs_todos()` | Seleciona todos os DBCs. |
| 7618 | `TabWonApp._dbcs_limpar()` | Desmarca todos os DBCs. |
| 7624 | `TabWonApp._qtd_dbcs_tabulados()` | Quantos arquivos o log atual usou. |
| 7638 | `TabWonApp._confirmar_fechamento()` | Pergunta antes de fechar o programa. |
| 7665 | `TabWonApp._ref_coluna(nome)` | Referência de uma coluna do resultado (letra estilo Excel). |
| 7672 | `TabWonApp._rotulo_coluna(nome)` | Rótulo 'A: Nome' de uma coluna. |
| 7675 | `TabWonApp._snapshot_estado()` | Fotografa o estado do resultado (tabela, formatos, gráficos, ajustes) para desfazer. |
| 7699 | `TabWonApp._registrar_snapshot_base()` | Empilha uma fotografia no histórico (até 60). |
| 7711 | `TabWonApp._aplicar_snapshot(snap)` | Restaura uma fotografia do histórico. |
| 7747 | `TabWonApp._atualizar_botoes_undo_redo()` | Habilita/desabilita desfazer e refazer. |
| 7765 | `TabWonApp._desfazer_base()` | Desfazer (implementação base). |
| 7770 | `TabWonApp._refazer_base()` | Refazer (implementação base). |
| 7775 | `TabWonApp._registrar_ajuste(texto_ato, gravar_snapshot)` | Registra um ato no superlog e tira a fotografia para desfazer. |
| 7786 | `TabWonApp._atualizar_titulo_export()` | Atualiza o título exibido acima da tabela. |
| 7797 | `TabWonApp._atualizar_visibilidade_super_log()` | Mostra o botão Copiar Super Log só quando há ajustes ou gráficos. |
| 7807 | `TabWonApp._ato_criar_coluna(nome, tipo, valor, modo, fmt)` | Monta o texto do ato 'criar coluna'. |
| 7811 | `TabWonApp._ato_formatar_coluna(nome, fmt)` | Monta o ato 'formatar coluna'. |
| 7815 | `TabWonApp._ato_renomear_coluna(de, para)` | Monta o ato 'renomear coluna'. |
| 7818 | `TabWonApp._ato_mover_coluna(nome, posicao_1based)` | Monta o ato 'mover coluna'. |
| 7821 | `TabWonApp._ato_excluir_coluna(nome)` | Monta o ato 'excluir coluna'. |
| 7824 | `TabWonApp._ato_excluir_linha(idx_1based, rotulo)` | Monta o ato 'excluir linha'. |
| 7832 | `TabWonApp._ato_ordenar(nome, asc)` | Monta o ato 'ordenar'. |
| 7835 | `TabWonApp._ato_operacao(tipo, colunas, valor_extra, destino, fmt, acao_data, formula)` | Monta o ato de uma operação (+ − × ÷ % datas). |
| 7857 | `TabWonApp._ato_set_detalhes(chave, valor)` | Monta o ato de detalhes (título, cor, TOTAL...). |
| 7860 | `TabWonApp._ato_grafico_criar(cfg)` | Monta o ato 'criar gráfico'. |
| 7863 | `TabWonApp._ato_grafico_excluir(gid)` | Monta o ato 'excluir gráfico'. |
| 7866 | `TabWonApp._aplicar_ato_silencioso(linha_ato)` | Executa um ato do superlog sem abrir janelas (despacha para _ato_exec_*). |
| 7910 | `TabWonApp._ato_exec_grafico_criar(cfg)` | Recria um gráfico a partir do ato. |
| 7926 | `TabWonApp._ato_exec_grafico_excluir(kw)` | Exclui um gráfico a partir do ato. |
| 7932 | `TabWonApp._ato_exec_set_detalhes(kw)` | Aplica detalhes (título, cor, zebra, TOTAL...). |
| 7954 | `TabWonApp._ato_exec_renomear_primeira_coluna(kw)` | Renomeia a primeira coluna (categorias). |
| 7960 | `TabWonApp._ato_exec_mostrar_total(kw)` | Mostra/oculta a linha TOTAL. |
| 7974 | `TabWonApp._recalcular_total()` | Recalcula a linha TOTAL (somas, fórmulas de colunas calculadas, tipos compatíveis). |
| 8033 | `TabWonApp._cols_da_formula(formula, df_sem)` | Colunas usadas numa fórmula de coluna calculada. |
| 8054 | `TabWonApp._calcular_total_formula(formula, destino_col, df_sem, totais_parciais)` | Calcula o TOTAL de uma coluna calculada pela fórmula (ex.: % recalculada). |
| 8117 | `TabWonApp._ato_exec_criar_coluna(kw)` | Cria coluna com valor fixo ou autoincremento. |
| 8164 | `TabWonApp._ato_exec_formatar_coluna(kw)` | Aplica formato a uma coluna (convertendo valores se preciso). |
| 8182 | `TabWonApp._ato_exec_renomear_coluna(kw)` | Renomeia coluna. |
| 8190 | `TabWonApp._ato_exec_mover_coluna(kw)` | Move coluna para outra posição. |
| 8203 | `TabWonApp._ato_exec_excluir_coluna(kw)` | Exclui coluna. |
| 8210 | `TabWonApp._ato_exec_excluir_linha(kw)` | Exclui linha (por posição/rótulo). |
| 8228 | `TabWonApp._ato_exec_excluir_total(kw)` | Exclui a linha TOTAL. |
| 8233 | `TabWonApp._ato_exec_ordenar(kw)` | Ordena a tabela por uma coluna, mantendo o TOTAL no fim. |
| 8251 | `TabWonApp._resolver_lista_colunas_por_nome_ou_pos(colunas)` | Converte referências de colunas (nome ou posição) nos nomes atuais. |
| 8275 | `TabWonApp._calcular_operacao_pura(tipo, colunas, valor_extra, fmt, acao_data)` | Calcula o resultado de uma operação entre colunas. |
| 8325 | `TabWonApp._ato_exec_operacao(kw)` | Cria a coluna de uma operação. |
| 8377 | `TabWonApp._aplicar_ajustes_silencioso(atos)` | Aplica uma lista de atos (reprodução do superlog). |

## TabWonApp — janela e painel de gráficos

| Linha | Função | Descrição |
|---:|---|---|
| 8384 | `TabWonApp._calcular_largura_painel()` | Largura do painel de gráficos. |
| 8387 | `TabWonApp._abrir_painel_graficos()` | Mostra o painel de gráficos ao lado da tabela. |
| 8410 | `TabWonApp._fechar_painel_graficos()` | Recolhe o painel de gráficos. |
| 8422 | `TabWonApp._reflow_barra(forcar)` | Reorganiza a barra de ferramentas do resultado conforme a largura da janela. |
| 8451 | `TabWonApp._atualizar_btn_ver_graficos()` | Mostra/esconde o botão Ver gráficos. |
| 8482 | `TabWonApp._atualizar_visibilidade_painel_graficos()` | Abre/fecha o painel conforme existam gráficos. |
| 8497 | `TabWonApp._habilitar_scroll_graficos()` | Liga a rolagem do mouse no painel de gráficos. |
| 8534 | `TabWonApp._on_window_configure(event)` | Reage ao redimensionamento/troca de monitor da janela. |
| 8556 | `TabWonApp._aplicar_restauracao_85()` | Ao sair de maximizado, restaura a janela em 85% da tela (ou no tamanho mínimo se arrastando). |
| 8569 | `TabWonApp._ajustar_ao_trocar_tela()` | Reajusta o tamanho ao mover a janela para outro monitor. |
| 8580 | `TabWonApp._monitor_da_janela()` | Identifica o monitor onde a janela está (Windows). |
| 8589 | `TabWonApp._botao_mouse_pressionado()` | Diz se o botão esquerdo do mouse está pressionado (Windows). |

## TabWonApp — editores, Consultor SQL e conversor

| Linha | Função | Descrição |
|---:|---|---|
| 8597 | `TabWonApp._atualizar_lista_defs()` | Recarrega a lista de DEFs. |
| 8605 | `TabWonApp.abrir_editor_def(texto_inicial, ao_salvar, pai)` | Editor de DEF completo (abrir, novo, gerar de DBC, inserir linha, analisar, salvar). |
| 9087 | `TabWonApp.abrir_editor_cnv(caminho, ao_fechar, pai)` | Editor de CNV completo (linhas, validação, correção guiada, gerar de DBC/DBF). |
| 9640 | `TabWonApp.abrir_consultor_sql(consulta)` | Consultor SQL passo a passo (início, arquivos, colunas, condições, ordem, SQL e resultado). |
| 10635 | `TabWonApp.abrir_conversor_dbf_dbc()` | Conversor DBF ⟺ DBC (listas, conversão, CRC, integridade, corrigir local, importar...). |

## TabWonApp — menu, tema, idioma, recentes, baixados e configurações

| Linha | Função | Descrição |
|---:|---|---|
| 11504 | `TabWonApp._abrir_menu_extra(event)` | Monta e abre o menu ⚙ (Shift + clique abre as configurações avançadas). |
| 11531 | `TabWonApp._aplicar_tema(arquivo, modo, salvar)` | Troca paleta/modo, recolore tudo, reaplica ícones e opcionalmente salva. |
| 11551 | `TabWonApp._rerenderizar_tela()` | Redesenha a tela atual após troca de tema. |
| 11575 | `TabWonApp._atualizar_botao_tema()` | Atualiza o ícone de modo de cor (botão não exibido na barra). |
| 11588 | `TabWonApp._alternar_tema_visual()` | Alterna claro → escuro → automático. |
| 11592 | `TabWonApp._vigiar_tema_auto()` | No modo automático, confere o modo do Windows a cada hora. |
| 11603 | `TabWonApp._paletas_disponiveis()` | Paletas liberadas no INI. |
| 11611 | `TabWonApp._aplicar_idioma()` | Ativa o idioma escolhido e retraduz a interface. |
| 11624 | `TabWonApp._montar_idioma_config(corpo, top)` | Bloco de idioma das Configurações do usuário. |
| 11671 | `TabWonApp._montar_aba_idioma(aba_, cfg)` | Aba Idioma das Configurações avançadas (idiomas disponíveis e padrão). |
| 11736 | `TabWonApp.finalizar_sucesso(*a, **k)` | Envolve _finalizar_sucesso_base: cria/reaproveita o registro de Últimas pesquisas. |
| 11749 | `TabWonApp._registrar_snapshot()` | Envolve a fotografia de desfazer e agenda salvar a pesquisa recente. |
| 11754 | `TabWonApp.desfazer()` | Desfazer (Ctrl+Z) e atualizar a pesquisa recente. |
| 11757 | `TabWonApp.refazer()` | Refazer (Ctrl+Y) e atualizar a pesquisa recente. |
| 11760 | `TabWonApp._agendar_recente()` | Agenda (com atraso) a gravação da pesquisa recente. |
| 11768 | `TabWonApp._salvar_recente()` | Grava a pesquisa atual (superlog completo) nas Últimas pesquisas. |
| 11791 | `TabWonApp.abrir_recentes()` | Janela Últimas pesquisas (executar, tirar da lista). |
| 11843 | `TabWonApp._executar_recente(e)` | Carrega o superlog de uma pesquisa recente e executa direto. |
| 11873 | `TabWonApp._registrar_baixado(caminho)` | Acrescenta um arquivo salvo à lista de Baixados. |
| 11883 | `TabWonApp._atualizar_botao_baixados()` | Mostra/esconde os botões Baixados (topo e tela de resultado). |
| 11898 | `TabWonApp.abrir_baixados()` | Janela Baixados (abrir arquivo/pasta, tirar da lista, limpar). |
| 11979 | `TabWonApp._montar_icones_config(f_ap, linha)` | Combobox de modelo de ícones com prévia nas Configurações. |
| 12025 | `TabWonApp.abrir_modal_configuracoes()` | Configurações do usuário (aparência, idioma, nome, excluir preferências). |

## TabWonApp — resultado SQL, Sobre e Ajuda

| Linha | Função | Descrição |
|---:|---|---|
| 12205 | `TabWonApp.executar_consulta_sql(nome, sql, tabelas, modelo)` | Executa uma consulta SQL salva e mostra o resultado na tela de tabulação. |
| 12249 | `TabWonApp._erro_consulta_sql(m)` | Mostra erro de consulta SQL. |
| 12256 | `TabWonApp._finalizar_consulta_sql(df, dt)` | Exibe o resultado de uma consulta SQL. |
| 12283 | `TabWonApp._sair_resultado_sql()` | Sai do modo resultado SQL e restaura a barra superior. |
| 12295 | `TabWonApp._salvar_resultado_sql()` | Exporta o resultado da consulta SQL. |
| 12311 | `TabWonApp._executar_entrada_sql(entry)` | Executa uma consulta salva do tipo Q ▶. |
| 12317 | `TabWonApp._abrir_sobre()` | Abre o Sobre no idioma ativo. |
| 12320 | `TabWonApp._abrir_ajuda()` | Abre a Ajuda no idioma ativo. |
| 12323 | `TabWonApp._abrir_modal_sobre(caminho)` | Janela Sobre (logo e texto do arquivo TabWon_sobre). |
| 12401 | `TabWonApp._abrir_modal_ajuda(caminho)` | Janela de Ajuda com seções, busca e negrito (*texto*). |

## TabWonApp — tela inicial, seleção e log

| Linha | Função | Descrição |
|---:|---|---|
| 12661 | `TabWonApp.setup_ui()` | Cria a estrutura principal da janela (barra superior, área central, rodapé). |
| 12724 | `TabWonApp._criar_botoes_tela_selecao()` | Cria os botões Limpar seleções e Início. |
| 12741 | `TabWonApp._remover_botoes_tela_selecao()` | Remove esses botões. |
| 12749 | `TabWonApp.limpar_selecoes()` | Volta ao DEF puro (zera ajustes, gráficos, seleções e travas). |
| 12815 | `TabWonApp.ir_para_inicio()` | Volta à tela inicial. |
| 12867 | `TabWonApp._texto_saudacao()` | Texto de saudação com o nome do usuário. |
| 12871 | `TabWonApp._atualizar_saudacao()` | Atualiza a saudação. |
| 12877 | `TabWonApp._erro_callback(tipo, valor, tb)` | Captura erros não tratados do Tk e grava no registro. |
| 12883 | `TabWonApp._verificar_boas_vindas()` | No primeiro uso, pede o nome do usuário. |
| 12952 | `TabWonApp.mostrar_welcome()` | Monta a tela inicial (logo, DEF, consultas salvas, colar log). |
| 13118 | `TabWonApp._criar_widgets_frame_main()` | Cria as listas de Linhas, Colunas, Incremento, Seleções, Arquivos e o log. |
| 13238 | `TabWonApp._travar_listbox(lb)` | Impede alterar uma lista (superlog travado). |
| 13251 | `TabWonApp._travar_dbcs_por_log(quantidade)` | Exige a quantidade de DBCs definida pelo log. |
| 13257 | `TabWonApp._destravar_dbcs()` | Libera a escolha de DBCs. |
| 13264 | `TabWonApp._aplicar_travamento_super_log(travado)` | Trava/destrava as listas quando um superlog está carregado. |
| 13306 | `TabWonApp._aplicar_cores_grupos()` | Aplica as cores de grupo definidas no DEF às listas. |
| 13332 | `TabWonApp._on_paste_log(event)` | Ao colar na caixa de log, aplica automaticamente. |
| 13335 | `TabWonApp._aplicar_log_do_texto()` | Aplica o log digitado/colado na tela inicial. |
| 13347 | `TabWonApp._atualizar_log_preview(force)` | Atualiza o log exibido conforme as escolhas da tela de seleção. |
| 13410 | `TabWonApp._match_incremento(nome_log)` | Acha na lista o incremento citado no log. |
| 13421 | `TabWonApp.aplicar_log(texto)` | Aplica um log/superlog: carrega o DEF, marca opções, seleções, arquivos e guarda os ajustes. |
| 13569 | `TabWonApp._tem_ajustes_no_texto(texto)` | Diz se um texto de log contém [Ajustes]. |
| 13573 | `TabWonApp.carregar_logs_salvos()` | Carrega a lista de consultas salvas no seletor. |
| 13600 | `TabWonApp.on_log_salvo_selected(event)` | Carrega a consulta salva escolhida. |
| 13642 | `TabWonApp._salvar_logs_arquivo()` | Regrava o arquivo de consultas salvas (com trava e backup). |

## TabWonApp — novidades, atalhos, zoom, relatório e lote

| Linha | Função | Descrição |
|---:|---|---|
| 13673 | `TabWonApp.abrir_novidades(automatico)` | Janela Novidades da versão. |
| 13693 | `TabWonApp._ver_novidades_se_preciso()` | Controle de exibição das novidades (hoje só pelo menu). |
| 13700 | `TabWonApp.abrir_atalhos()` | Janela de atalhos de teclado. |
| 13726 | `TabWonApp._na_tela_principal()` | Diz se nenhuma janela modal está aberta. |
| 13733 | `TabWonApp._no_resultado()` | Diz se a tela de resultado está ativa. |
| 13737 | `TabWonApp._atalho(acao)` | Executa a ação de um atalho conforme a tela. |
| 13760 | `TabWonApp._buscar_proximo()` | F3: próximo resultado da busca. |
| 13768 | `TabWonApp._instalar_atalhos()` | Liga os atalhos de teclado. |
| 13781 | `TabWonApp._altura_linha()` | Altura da linha da tabela conforme o zoom. |
| 13784 | `TabWonApp._fonte_tabela()` | Fonte da tabela conforme o zoom. |
| 13787 | `TabWonApp._zoom_tabela(passo)` | Aumenta/diminui/restaura o zoom da tabela e salva. |
| 13796 | `TabWonApp.gerar_relatorio_problema()` | Gera o ZIP de relatório de problema. |
| 13843 | `TabWonApp.abrir_lote()` | Janela Rodar consultas em lote. |
| 13906 | `TabWonApp._iniciar_lote(itens, pasta, abrir)` | Começa a execução em lote. |
| 13938 | `TabWonApp._lote_proxima()` | Executa a próxima consulta do lote. |
| 13969 | `TabWonApp._lote_vigiar()` | Acompanha o fim de cada tabulação do lote e grava o XLSX. |
| 14002 | `TabWonApp._lote_concluiu(ok, msg)` | Registra o resultado de uma consulta do lote. |
| 14006 | `TabWonApp._lote_fim()` | Mostra o resumo do lote. |

## TabWonApp — consultas, DEF, seleções e processamento

| Linha | Função | Descrição |
|---:|---|---|
| 14024 | `TabWonApp.gerenciar_consultas_salvas()` | Janela Gerenciar (carregar, renomear, excluir, editar SQL). |
| 14164 | `TabWonApp.mostrar_campos(mostrar)` | Mostra/esconde a área de seleção. |
| 14175 | `TabWonApp.verificar_pendencias()` | Lista o que falta escolher para executar. |
| 14192 | `TabWonApp._ajustar_modo_incremento()` | Incremento múltiplo com Coluna 'Não ativa'; único com Coluna ativa. |
| 14204 | `TabWonApp.validar_botao_executar()` | Habilita o botão Executar Tabulação quando não há pendências. |
| 14210 | `TabWonApp.carregar_defs()` | Lista os DEFs da pasta do TabWon. |
| 14217 | `TabWonApp.on_def_selected(event)` | Carrega o DEF escolhido e preenche todas as listas. |
| 14346 | `TabWonApp.on_coluna_selected(event)` | Reage à escolha de coluna. |
| 14353 | `TabWonApp.on_incremento_selected(event)` | Reage à escolha de incremento. |
| 14360 | `TabWonApp.atualizar_lista_ativa(idx)` | Atualiza a lista de seleções ativas. |
| 14376 | `TabWonApp.incluir_selecao()` | Move uma seleção para Ativas. |
| 14397 | `TabWonApp.excluir_selecao()` | Tira uma seleção de Ativas. |
| 14419 | `TabWonApp.configurar_filtro_ativo()` | Janela de valores de uma seleção (marcar categorias, buscar, colar códigos). |
| 14571 | `TabWonApp.executar_tabulacao()` | Valida e inicia o processamento em segundo plano com a tela de progresso. |
| 14638 | `TabWonApp._resetar_graficos()` | Apaga os gráficos. |
| 14645 | `TabWonApp._restaurar_estado()` | Repreenche as listas a partir do DEF carregado. |
| 14691 | `TabWonApp._serie_normalizada(df, coluna, n_chars, cache, pos)` | Coluna de códigos normalizada (com cache) para filtrar. |
| 14698 | `TabWonApp._serie_e_conjunto_filtro(df, item, nome_sel, cache_series, cache_codes)` | Coluna e conjunto de códigos aceitos de uma seleção. |
| 14724 | `TabWonApp._codigos_do_filtro(nome_sel, cache_codes)` | Códigos aceitos de uma seleção. |
| 14730 | `TabWonApp._colunas_necessarias(linha_info, coluna_info, incrementos, filtros)` | Lista as colunas do DBC realmente necessárias (leitura mais rápida). |
| 14744 | `TabWonApp._avisar_conversao(qtd)` | Informa que DBCs estão sendo convertidos para o cache. |
| 14754 | `TabWonApp.processar_dados(linha_info, coluna_info, incrementos, dbcs_escolhidos, filtros, ign_linhas, ign_colunas)` | Lê os DBCs, aplica filtros e CNVs e monta a tabela cruzada (roda em thread). |

## TabWonApp — tela de resultado e gráficos

| Linha | Função | Descrição |
|---:|---|---|
| 14915 | `TabWonApp._finalizar_sucesso_base(pivot, regs, tempo, erros, total_original, tempo_seg)` | Mostra a tela de resultado (barras, tabela, log, painel) e reaplica ajustes do superlog. |
| 15223 | `TabWonApp._render_graficos()` | Desenha todos os gráficos no painel. |
| 15257 | `TabWonApp._grafico_tem_incompatibilidade(cfg)` | Verifica se o gráfico aponta para colunas inexistentes ('Necessário editar'). |
| 15288 | `TabWonApp._normalizar_eixos_cfg(cfg)` | Ajusta a configuração de eixos do gráfico às colunas atuais. |
| 15332 | `TabWonApp._render_um_grafico_accordion(cfg)` | Cria o card de um gráfico (cabeçalho, editar, excluir, rodapé). |
| 15431 | `TabWonApp._preencher_rodape(txt_widget, texto)` | Escreve o rodapé do gráfico com negrito. |
| 15443 | `TabWonApp._obter_formato_label(coluna)` | Formato de uma coluna para rótulos do gráfico. |
| 15448 | `TabWonApp._desenhar_grafico_inline(cfg, ax, fig, canvas)` | Desenha os tipos clássicos (barras, pizza, área, linhas, dispersão, empilhadas...). |
| 15815 | `TabWonApp._desenhar_ref_estat(ax, cfg, tipo, serie, labels)` | Desenha média, mediana, faixa de desvio e tendência no gráfico. |
| 15842 | `TabWonApp._copiar_grafico_png_cfg(gid)` | Copia o gráfico como imagem com rodapé. |
| 15921 | `TabWonApp._excluir_grafico_cfg(gid)` | Exclui um gráfico. |
| 15932 | `TabWonApp._editar_grafico_cfg(gid)` | Abre o modal para editar um gráfico. |
| 15940 | `TabWonApp._obter_nomes_x()` | Opções de eixo X. |
| 15946 | `TabWonApp._obter_colunas_numericas()` | Colunas numéricas disponíveis para eixo Y. |
| 15951 | `TabWonApp._sugerir_nome_grafico(ignorar_id)` | Sugere 'Gráfico N' sem repetir. |
| 15963 | `TabWonApp._titulo_grafico_em_uso(titulo, ignorar_id)` | Verifica título de gráfico repetido. |
| 15972 | `TabWonApp.abrir_modal_grafico(cfg_editando)` | Modal Criar/Editar gráfico (tipos, eixos, opções, estatística, rodapé). |
| 16449 | `TabWonApp._dados_gauge(cfg, df_sem, cols_sel)` | Prepara os valores do velocímetro e do linear gauge. |
| 16489 | `TabWonApp._desenhar_grafico_novo(tipo, cfg, df_sem, ax, fig, canvas)` | Desenha os tipos novos (histograma, Pareto, mapa de calor, gauges, boxplot, pirâmide, treemap...). |

## TabWonApp — tabela de resultado

| Linha | Função | Descrição |
|---:|---|---|
| 16776 | `TabWonApp._refresh_log_text_widget()` | Atualiza a caixa de log do resultado. |
| 16786 | `TabWonApp._valor_para_exibir(valor, formato, index_eh_total)` | Texto de uma célula conforme o formato da coluna. |
| 16791 | `TabWonApp._autoajustar_colunas_tree()` | Ajusta a largura das colunas ao conteúdo. |
| 16807 | `TabWonApp._render_resultado()` | Preenche a tabela de resultado (formatos, zebra, destaques, TOTAL). |
| 16907 | `TabWonApp._on_tree_right_click(event)` | Menu de contexto da tabela. |
| 16945 | `TabWonApp._ctx_ocultar_total()` | Oculta/mostra o TOTAL pelo menu de contexto. |
| 16954 | `TabWonApp._ctx_excluir_linha(label_linha)` | Exclui linha pelo menu de contexto. |
| 16970 | `TabWonApp._ctx_excluir_coluna(nome_col)` | Exclui coluna pelo menu de contexto. |
| 16981 | `TabWonApp._ctx_formatar_coluna(nome_col)` | Formata coluna pelo menu de contexto. |
| 16985 | `TabWonApp._on_tree_double_click(event)` | Duplo clique: formatar coluna / detalhes. |
| 16992 | `TabWonApp._on_heading_press(event)` | Início de clique/arraste no cabeçalho. |
| 17002 | `TabWonApp._on_heading_motion(event)` | Arraste de coluna pelo cabeçalho. |
| 17011 | `TabWonApp._on_heading_release(event)` | Solta: ordena (clique) ou move a coluna (arraste). |
| 17040 | `TabWonApp._redesenhar_letras()` | Atualiza a linha de letras (A, B, C) acima da tabela. |
| 17062 | `TabWonApp._redesenhar_numeros()` | Atualiza a numeração das linhas. |

## TabWonApp — formatação, Funções, transposição e busca

| Linha | Função | Descrição |
|---:|---|---|
| 17084 | `TabWonApp._ordenar_por(coluna)` | Ordena pela coluna clicada (alternando crescente/decrescente). |
| 17104 | `TabWonApp._opcoes_colunas_para(operacao)` | Colunas válidas para uma operação. |
| 17115 | `TabWonApp.abrir_modal_formatar_coluna_simples(col_nome)` | Modal rápido de formatação de uma coluna. |
| 17158 | `TabWonApp.abrir_modal_detalhes()` | Modal Detalhes da tabulação. |
| 17271 | `TabWonApp._ato_calculo(calc, colunas, params, destinos, fmt)` | Monta o ato de uma Função. |
| 17278 | `TabWonApp._nome_unico(base, reservados)` | Gera nome de coluna único (acrescenta 2, 3...). |
| 17286 | `TabWonApp._rotulo_primeira()` | Rótulo da primeira coluna. |
| 17290 | `TabWonApp._texto_da_coluna(df_sem, col)` | Valores de uma coluna como texto (inclui a 1ª coluna). |
| 17305 | `TabWonApp._calcular_texto(calc, df_sem, cols, params)` | Funções de texto (esquerda, direita, meio, concatenar). |
| 17328 | `TabWonApp._resolver_cols_calculo(cols)` | Resolve as colunas de uma Função. |
| 17337 | `TabWonApp._destinos_calculo(calc, nome, cols, params)` | Nomes das colunas criadas por uma Função. |
| 17343 | `TabWonApp._aplicar_calculo(calc, cols, params, destinos, fmt)` | Cria as colunas de uma Função. |
| 17366 | `TabWonApp._ato_exec_calculo(kw)` | Executa uma Função a partir do ato. |
| 17385 | `TabWonApp.abrir_modal_calculos()` | Abre as Funções (atalho). |
| 17388 | `TabWonApp.abrir_modal_funcoes()` | Modal Funções (ƒ). |
| 17726 | `TabWonApp._transpor_resultado(ativo)` | Troca linhas por colunas (e volta). |
| 17764 | `TabWonApp._ato_exec_transpor(kw)` | Executa a transposição a partir do ato. |
| 17769 | `TabWonApp._tag_destaque(rotulo)` | Tag de cor de um destaque da busca. |
| 17775 | `TabWonApp._configurar_tags_destaque()` | Configura as cores dos destaques. |
| 17780 | `TabWonApp._atualizar_botoes_destaque()` | Mostra/esconde Limpar destaques e Subir/Descer destacados. |
| 17798 | `TabWonApp.limpar_destaques()` | Remove os destaques. |
| 17803 | `TabWonApp.mover_destacados()` | Leva as linhas destacadas para o topo/fim. |
| 17818 | `TabWonApp._mover_linhas(rotulos, posicao)` | Move linhas para o topo ou o fim. |
| 17830 | `TabWonApp._ato_exec_mover_linhas(kw)` | Executa o ato de mover linhas. |
| 17837 | `TabWonApp._remover_marca_busca()` | Tira a marca da célula encontrada. |
| 17844 | `TabWonApp._marcar_celula(item, coluna_idx, texto)` | Marca a célula encontrada. |
| 17858 | `TabWonApp.abrir_busca()` | Janela Buscar na tabela. |

## TabWonApp — configurações avançadas e Estatística

| Linha | Função | Descrição |
|---:|---|---|
| 17969 | `TabWonApp.abrir_config_avancadas()` | Configurações avançadas (menu, consultas, identificadores, cores, ícones, usuários, idioma). |
| 18211 | `TabWonApp._ato_estatistica(est, colunas, params, destinos, fmt)` | Monta o ato de uma estatística. |
| 18219 | `TabWonApp._calcular_estat_saidas(est, cols_reais, params)` | Calcula as colunas de uma estatística. |
| 18239 | `TabWonApp._nomes_destinos(est, nome, params)` | Nomes das colunas criadas por uma estatística. |
| 18253 | `TabWonApp._aplicar_saidas_estat(est, cols_reais, params, destinos, fmt, saidas)` | Cria as colunas da estatística. |
| 18270 | `TabWonApp._ato_exec_estatistica(kw)` | Executa uma estatística a partir do ato. |
| 18288 | `TabWonApp._mostrar_relatorio_estat(titulo, texto, pai)` | Janela de relatório estatístico com Copiar. |
| 18316 | `TabWonApp.abrir_modal_estatistica()` | Modal Estatística (Ø). |

## TabWonApp — operações e edição de colunas

| Linha | Função | Descrição |
|---:|---|---|
| 18562 | `TabWonApp.abrir_modal_operacao(operacao)` | Modal de operação (+ − × ÷ % datas). |
| 18743 | `TabWonApp.abrir_modal_adicionar_coluna()` | Modal Adicionar coluna. |
| 18870 | `TabWonApp.abrir_modal_editar_colunas()` | Modal Editar/Mover colunas. |
| 18963 | `TabWonApp.abrir_modal_excluir()` | Modal Excluir linhas/colunas. |

## TabWonApp — log, cópia, exportação e nova consulta

| Linha | Função | Descrição |
|---:|---|---|
| 19030 | `TabWonApp._texto_log(incluir_ajustes, incluir_graficos)` | Texto do log com ou sem ajustes/gráficos. |
| 19042 | `TabWonApp._avisar_exigencia_arquivos()` | Avisa quando a consulta exige uma quantidade fixa de arquivos. |
| 19077 | `TabWonApp._texto_log_sem_graficos()` | Log sem a seção de gráficos. |
| 19091 | `TabWonApp._texto_log_com_tag(incluir_ajustes, incluir_graficos)` | Log com a marcação de superlog. |
| 19100 | `TabWonApp._nome_ja_existe(nome, ignorar_idx)` | Verifica nome de consulta repetido. |
| 19106 | `TabWonApp.salvar_log()` | Janela Salvar consulta. |
| 19180 | `TabWonApp.copiar_log()` | Copia o log simples. |
| 19193 | `TabWonApp.copiar_super_log()` | Copia o superlog. |
| 19201 | `TabWonApp.abrir_modal_copiar()` | Menu/modal de cópia da tabela. |
| 19261 | `TabWonApp._texto_puro_vertical()` | Tabela como texto puro vertical (WhatsApp). |
| 19281 | `TabWonApp._copiar_imagem_tabela()` | Gera PNG da tabela + log e copia. |
| 19379 | `TabWonApp._montar_log(regs, tempo, erros)` | Monta o log da tabulação (opções, seleções, arquivos, registros, tempo). |
| 19420 | `TabWonApp._abrir_escolha_campos_registros()` | Janela Escolhe Campos de Salvar registros. |
| 19579 | `TabWonApp._executar_exportacao_registros(caminho, campos, tamanhos, ctx)` | Executa a gravação dos registros com progresso. |
| 19641 | `TabWonApp._filtrar_registros(df, ctx)` | Aplica os filtros da consulta aos registros. |
| 19662 | `TabWonApp._gravar_registros_xlsx(caminho, campos, tamanhos, ctx, progresso, cancelado)` | Grava os registros em XLSX. |
| 19739 | `TabWonApp._sanitizar_nome_planilha(nome)` | Limpa o nome da aba do Excel. |
| 19744 | `TabWonApp._exportar_com_log(caminho, formato)` | Grava XLSX/CSV com tabela, formatos e log. |
| 19815 | `TabWonApp._mostrar_sucesso_export(caminho, abrir_arquivo, abrir_pasta, parent)` | Janela 'Salvo com sucesso' (abrir arquivo/diretório) e registra em Baixados. |
| 19838 | `TabWonApp.exportar_xlsx()` | Exportar XLSX (Ctrl+E). |
| 19855 | `TabWonApp.exportar_csv()` | Exportar CSV (Ctrl+Shift+E). |
| 19872 | `TabWonApp._html_tabela(incluir_log)` | Tabela em HTML (cópia formatada). |
| 19909 | `TabWonApp._html_clipboard(html)` | Coloca HTML na área de transferência do Windows. |
| 19932 | `TabWonApp.nova_consulta()` | Volta à seleção mantendo o superlog e os gráficos. |
| 20028 | `TabWonApp.finalizar_erro(msg)` | Mostra erro do processamento e volta à seleção. |

_Total: 559 funções, métodos e classes._
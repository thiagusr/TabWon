# TabWon — loader (`TabWon.exe`)

O loader é o programa que abre o TabWon na versão compilada. Ele aparece na tela no mesmo instante do clique, mostra o logotipo, uma mensagem de andamento e uma barra animada, e inicia o tabulador (`TabWon_tabulador.exe`). Quando a janela do TabWon fica pronta, o loader se fecha sozinho.

Ele existe porque o tabulador precisa carregar as bibliotecas da pasta `TabWon_dependencias` antes de mostrar a janela. Na primeira abertura, ou quando o TabWon está numa pasta da rede, isso pode levar cerca de um minuto, e sem o loader o usuário não via nada acontecer.

O loader só existe na versão compilada. Quem roda o `TabWon.py` direto pelo Python não precisa dele.

## Como funciona

1. Os atalhos e o instalador apontam sempre para o **`TabWon.exe`**, que é o loader.
2. Assim que a tela aparece, o loader cria um sinal do Windows (evento nomeado), guarda o nome dele na variável de ambiente `TABWON_LOADER_EVENTO` e inicia o `TabWon_tabulador.exe` da mesma pasta, repassando os argumentos recebidos.
3. Quando termina de montar a janela, o `TabWon.py` aciona esse sinal (função `_avisar_loader`), e o loader se fecha.
4. O loader também se fecha se o tabulador for encerrado, se a janela do TabWon estiver aberta há mais de 20 segundos sem o sinal ou depois de 5 minutos.

A tela aparece centralizada no monitor onde está o mouse, com os textos em português do Brasil. O loader mostra a tela primeiro e só depois inicia o tabulador, para que ela apareça logo após o clique. O loader não mostra a versão do TabWon. A versão que aparece nas propriedades do arquivo (1.0.0.0) é a do próprio loader.

## Arquivos desta pasta

| Arquivo | Para quê |
|---|---|
| `TabWon_loader.cs` | Código-fonte em C#. |
| `TabWon_loader.manifest` | Manifesto do Windows: roda sem administrador e com nitidez em telas com zoom. |
| `TabWon_loader.png` | Logotipo exibido na tela de carregamento (vai embutido no `.exe`). |
| `compilar_loader.bat` | Compila o loader com um duplo clique. |
| `saida\` | Onde o `TabWon.exe` é gerado. Fica fora do repositório. |

O ícone é o mesmo do programa, `TabWon_configs\TabWon_icos.ico`, e também vai embutido no `.exe`.

## Como compilar

**Não precisa instalar nada.** O loader é feito para o **.NET Framework 4.8**, que já vem no Windows 10 e no Windows 11. O compilador C# (`csc.exe`) também já vem com ele, em `C:\Windows\Microsoft.NET\Framework64\v4.0.30319`.

1. Confira se a pasta `TabWon_configs` (com o `TabWon_icos.ico`) está ao lado da pasta `loader`, como no repositório.
2. Dê **duplo clique em `compilar_loader.bat`**.
3. O resultado fica em **`loader\saida\TabWon.exe`**.

O compilador do Windows pode mostrar um aviso de que só aceita até o C# 5. É normal: o código foi escrito para essa versão.

O `compilar.bat` da pasta principal já compila o loader no fim da compilação do TabWon e copia o `TabWon.exe` para `dist\TabWon`. O `compilar_loader.bat` serve para compilar só o loader.

## Onde o loader fica

```
TabWon/
├─ TabWon.exe              ← loader (esta pasta)
├─ TabWon_tabulador.exe    ← programa principal (gerado pelo PyInstaller)
├─ TabWon_dependencias/
└─ TabWon_configs/
```

Se o `TabWon_tabulador.exe` não estiver ao lado do loader, ele avisa e não abre nada.

## Ideias para o futuro

* Verificar, numa pasta da rede ou no site, se há uma versão nova do TabWon antes de abrir.
* Mostrar etapas reais do carregamento enviadas pelo tabulador.

## Licença e autoria

O loader faz parte do projeto TabWon e segue a mesma licença, **GNU GPL v3**, e o mesmo termo adicional de autoria descrito no [`AVISO.md`](../AVISO.md) da pasta principal.

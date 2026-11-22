@echo off
chcp 65001 >nul
setlocal
cd /d "%~dp0"
echo ============================================
echo  TabWon - gerar o executavel
echo ============================================
if not exist "TabWon.py" ( echo Falta o TabWon.py nesta pasta. & pause & exit /b 1 )
if not exist "TabWon_configs\TabWon_icos.ico" ( echo Falta TabWon_configs\TabWon_icos.ico & pause & exit /b 1 )
if not exist "loader\compilar_loader.bat" ( echo Falta a pasta loader com o compilar_loader.bat & pause & exit /b 1 )

if not exist ".venv_build\Scripts\python.exe" (
    echo Criando ambiente limpo de compilacao...
    py -3 -m venv .venv_build || python -m venv .venv_build || ( echo Python nao encontrado. & pause & exit /b 1 )
)
call ".venv_build\Scripts\activate.bat"
python -m pip install --upgrade pip
python -m pip install pandas duckdb pyarrow openpyxl matplotlib pillow pywin32 pyinstaller
if errorlevel 1 ( echo Falha ao instalar os pacotes. & pause & exit /b 1 )

if exist build rmdir /s /q build
if exist dist rmdir /s /q dist
pyinstaller --noconfirm --clean TabWon.spec
if errorlevel 1 ( echo Falha na compilacao. & pause & exit /b 1 )

echo.
echo Compilando o loader (TabWon.exe)...
set "TABWON_SEM_PAUSA=1"
call "loader\compilar_loader.bat"
if errorlevel 1 ( echo Falha ao compilar o loader. & pause & exit /b 1 )
copy /y "loader\saida\TabWon.exe" "dist\TabWon\TabWon.exe" >nul

echo.
echo Pronto! Foram gerados em dist\TabWon:
echo   TabWon.exe               (loader: abre a tela de carregamento e o tabulador)
echo   TabWon_tabulador.exe
echo   TabWon_configs
echo   TabWon_dependencias  (nao mexer)
echo Copie os executáveis e as referidas pastas acima para onde ficam os DEFs e demais arquivos do TabWin.
pause

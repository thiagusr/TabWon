@echo off
setlocal
cd /d "%~dp0"
echo ============================================
echo  TabWon - gerar o executavel
echo ============================================
if not exist "TabWon.py" ( echo Falta o TabWon.py nesta pasta. & pause & exit /b 1 )
if not exist "TabWon_configs\TabWon_icos.ico" ( echo Falta TabWon_configs\TabWon_icos.ico & pause & exit /b 1 )

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
echo Pronto! Foram gerados em dist\TabWon:
echo   TabWon.exe
echo   TabWon_configs
echo   TabWon_dependencias  (nao mexer)
echo Copie o executável e as referidas pastas acima para onde ficam os DEFs e demais arquivos do TabWin.
pause

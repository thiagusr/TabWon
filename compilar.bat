@echo off
chcp 65001 >nul
setlocal
cd /d "%~dp0"
set "COMPILADO=%~dp0Compilado"
set "SAIDA=%~dp0Compilado\dist\TabWon"
for %%I in ("%~dp0..") do set "DESTINO=%%~fI\TabWon_compilado"
echo ============================================
echo  TabWon - gerar o executável
echo ============================================
if not exist "TabWon.py" ( echo Falta o TabWon.py nesta pasta. & pause & exit /b 1 )
if not exist "TabWon_configs\TabWon_icos.ico" ( echo Falta TabWon_configs\TabWon_icos.ico & pause & exit /b 1 )
if not exist "loader\compilar_loader.bat" ( echo Falta a pasta loader com o compilar_loader.bat & pause & exit /b 1 )
for %%A in (AVISO.md README.md TabWon_LEIA-ME.txt) do if not exist "%%A" ( echo Falta o %%A nesta pasta. & pause & exit /b 1 )
if not exist "TabWon_configs\TabWonDBC.exe" echo Atenção: falta o TabWon_configs\TabWonDBC.exe ^(veja a seção 9 do README^).
if not exist "%COMPILADO%" mkdir "%COMPILADO%"

if not exist "%COMPILADO%\venv\Scripts\python.exe" (
    echo Criando ambiente limpo de compilação em Compilado\venv...
    py -3 -m venv "%COMPILADO%\venv" || python -m venv "%COMPILADO%\venv" || ( echo Python não encontrado. & pause & exit /b 1 )
)
call "%COMPILADO%\venv\Scripts\activate.bat"
python -m pip install --upgrade pip
python -m pip install pandas duckdb pyarrow openpyxl matplotlib pillow pywin32 pyinstaller
if errorlevel 1 ( echo Falha ao instalar os pacotes. & pause & exit /b 1 )

if exist "%COMPILADO%\build" rmdir /s /q "%COMPILADO%\build"
if exist "%COMPILADO%\dist" rmdir /s /q "%COMPILADO%\dist"
pyinstaller --noconfirm --clean --distpath "%COMPILADO%\dist" --workpath "%COMPILADO%\build" TabWon.spec
if errorlevel 1 ( echo Falha na compilação. & pause & exit /b 1 )
call deactivate

echo.
echo Compilando o loader (TabWon.exe)...
set "TABWON_SEM_PAUSA=1"
set "TABWON_LOADER_SAIDA=%COMPILADO%\loader"
call "loader\compilar_loader.bat"
if errorlevel 1 ( echo Falha ao compilar o loader. & pause & exit /b 1 )
copy /y "%COMPILADO%\loader\TabWon.exe" "%SAIDA%\TabWon.exe" >nul

echo.
echo Juntando os arquivos do TabWon...
robocopy "TabWon_configs" "%SAIDA%\TabWon_configs" /E /XD .usuarios .cache_dbc .backups_consultas desinstalar __pycache__ /XF TabWon_logs.log TabWon_logs_sql.log TabWon_cache_pasta.txt *.lock *.tmp *.lnk *.bak desktop.ini Thumbs.db /NFL /NDL /NJH /NJS /NP >nul
if errorlevel 8 ( echo Falha ao copiar a TabWon_configs. & pause & exit /b 1 )
if exist "TabWonDBC" (
    robocopy "TabWonDBC" "%SAIDA%\TabWonDBC" /E /XD bin obj .vs /NFL /NDL /NJH /NJS /NP >nul
    if errorlevel 8 ( echo Falha ao copiar a pasta TabWonDBC. & pause & exit /b 1 )
)
for %%A in (AVISO.md README.md TabWon_LEIA-ME.txt) do copy /y "%%A" "%SAIDA%\%%A" >nul
powershell -NoProfile -ExecutionPolicy Bypass -Command "$p = Join-Path $env:SAIDA 'TabWon_configs\TabWon_configs.ini'; if (Test-Path -LiteralPath $p) { $t = [IO.File]::ReadAllText($p); $t = [regex]::Replace($t, '(?m)^(\s*tabwin\s*=)[^#\r\n]*', '$1              '); [IO.File]::WriteAllText($p, $t, (New-Object Text.UTF8Encoding $false)) }"

echo.
echo Pronto! Foram gerados em Compilado\dist\TabWon:
echo   TabWon.exe               (loader: abre a tela de carregamento e o tabulador)
echo   TabWon_tabulador.exe
echo   TabWon_configs
echo   TabWon_dependencias  (não mexer)
echo   TabWonDBC, AVISO.md, README.md e TabWon_LEIA-ME.txt
echo.
echo Deseja manter apenas o que importa?
echo   S = copia para "%DESTINO%", sobrescrevendo só o que mudou, e apaga a pasta Compilado
echo   N = deixa tudo em Compilado\dist\TabWon
choice /c SN /n /m "Sua escolha [S/N]: "
if errorlevel 2 goto :manter

if not exist "%DESTINO%" mkdir "%DESTINO%"
robocopy "%SAIDA%\TabWon_dependencias" "%DESTINO%\TabWon_dependencias" /MIR /NFL /NDL /NJH /NJS /NP >nul
if errorlevel 8 ( echo Falha ao copiar a TabWon_dependencias para "%DESTINO%". Feche o TabWon e tente de novo. A pasta Compilado foi mantida. & pause & exit /b 1 )
robocopy "%SAIDA%" "%DESTINO%" /E /XD "%SAIDA%\TabWon_dependencias" /XF TabWon_configs.ini /NFL /NDL /NJH /NJS /NP >nul
if errorlevel 8 ( echo Falha ao copiar para "%DESTINO%". Feche o TabWon e tente de novo. A pasta Compilado foi mantida. & pause & exit /b 1 )
if not exist "%DESTINO%\TabWon_configs\TabWon_configs.ini" copy /y "%SAIDA%\TabWon_configs\TabWon_configs.ini" "%DESTINO%\TabWon_configs\TabWon_configs.ini" >nul
cd /d "%~dp0"
rmdir /s /q "%COMPILADO%"
echo.
echo Pronto! O TabWon compilado está em:
echo   %DESTINO%
echo Copie o conteúdo dessa pasta para onde ficam os DEFs e demais arquivos do TabWin.
pause
exit /b 0

:manter
echo.
echo Os arquivos continuam em Compilado\dist\TabWon.
echo Copie os executáveis e as referidas pastas para onde ficam os DEFs e demais arquivos do TabWin.
pause
exit /b 0

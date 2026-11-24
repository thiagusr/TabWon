@echo off
setlocal
cd /d "%~dp0"
echo ============================================
echo  TabWon - compilar o loader (TabWon.exe)
echo ============================================
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" ( echo Nao encontrei o compilador C# do .NET Framework 4.8 deste Windows. & goto :erro )
for %%A in (TabWon_loader.cs TabWon_loader.manifest TabWon_loader.png TabWon_loader_molecula.png) do if not exist "%%A" ( echo Falta loader\%%A & goto :erro )
if not exist "..\TabWon_configs\TabWon_icos.ico" ( echo Falta TabWon_configs\TabWon_icos.ico & goto :erro )
set "SAIDA_LOADER=saida"
if defined TABWON_LOADER_SAIDA set "SAIDA_LOADER=%TABWON_LOADER_SAIDA%"
if not exist "%SAIDA_LOADER%" mkdir "%SAIDA_LOADER%"
"%CSC%" /nologo /codepage:65001 /target:winexe /optimize+ /platform:anycpu /out:"%SAIDA_LOADER%\TabWon.exe" /win32icon:"..\TabWon_configs\TabWon_icos.ico" /win32manifest:"TabWon_loader.manifest" /resource:"TabWon_loader.png",TabWon_loader.png /resource:"TabWon_loader_molecula.png",TabWon_loader_molecula.png /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll "TabWon_loader.cs"
if errorlevel 1 ( echo Falha ao compilar o loader. & goto :erro )
echo.
echo Pronto: %SAIDA_LOADER%\TabWon.exe
if not defined TABWON_SEM_PAUSA pause
exit /b 0

:erro
if not defined TABWON_SEM_PAUSA pause
exit /b 1

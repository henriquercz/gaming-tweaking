@echo off
echo 🎮 Gaming Tweaks Manager - Instalador do .NET 8.0
echo ===================================================

echo Este script irá baixar e instalar o .NET 8.0 Runtime necessário
echo para executar o Gaming Tweaks Manager.
echo.
echo ⚠️  Você precisa de conexão com a internet para continuar.
echo.
pause

REM Criar diretório temporário
mkdir "%TEMP%\GamingTweaksManager" 2>nul
cd /d "%TEMP%\GamingTweaksManager"

echo 📥 Baixando .NET 8.0 Runtime...
echo.

REM Baixar o .NET 8.0 Desktop Runtime (x64)
powershell -Command "& {Invoke-WebRequest -Uri 'https://download.microsoft.com/download/6/0/f/60fc8ea7-d5d1-4c9c-b8f1-d0b5094c8db3/windowsdesktop-runtime-8.0.8-win-x64.exe' -OutFile 'dotnet-runtime-8.0-win-x64.exe'}"

if not exist "dotnet-runtime-8.0-win-x64.exe" (
    echo ❌ Erro ao baixar o .NET Runtime
    echo.
    echo Você pode baixar manualmente em:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo ✅ Download concluído!
echo.
echo 🔧 Instalando .NET 8.0 Runtime...
echo ⚠️  Uma janela de instalação será aberta. Siga as instruções.
echo.

REM Executar o instalador
start /wait dotnet-runtime-8.0-win-x64.exe

echo.
echo ✅ Instalação do .NET 8.0 concluída!
echo.
echo Agora você pode executar o Gaming Tweaks Manager.
echo.

REM Limpar arquivos temporários
del "dotnet-runtime-8.0-win-x64.exe" 2>nul

echo Pressione qualquer tecla para fechar...
pause >nul

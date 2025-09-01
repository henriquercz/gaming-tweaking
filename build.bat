@echo off
echo 🎮 Gaming Tweaks Manager - Script de Compilação
echo ================================================

REM Navegar para o diretório do script
cd /d "%~dp0"

REM Verificar se existe o arquivo de projeto
if not exist "GamingTweaksManager.csproj" (
    echo ❌ Arquivo de projeto não encontrado!
    echo Certifique-se de que o script está no diretório correto.
    echo Procurando por: GamingTweaksManager.csproj
    echo Diretório atual: %CD%
    pause
    exit /b 1
)

REM Verificar se o .NET SDK está instalado
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET SDK não encontrado!
    echo.
    echo Para compilar este projeto, você precisa instalar o .NET 8.0 SDK:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo ✅ .NET SDK encontrado
echo ✅ Arquivo de projeto encontrado
echo.

REM Restaurar pacotes NuGet
echo 📦 Restaurando pacotes NuGet...
dotnet restore "GamingTweaksManager.csproj"
if %errorlevel% neq 0 (
    echo ❌ Erro ao restaurar pacotes
    pause
    exit /b 1
)

echo ✅ Pacotes restaurados com sucesso
echo.

REM Compilar o projeto
echo 🔨 Compilando o projeto...
dotnet build "GamingTweaksManager.csproj" --configuration Release
if %errorlevel% neq 0 (
    echo ❌ Erro na compilação
    pause
    exit /b 1
)

echo ✅ Compilação concluída com sucesso!
echo.

REM Publicar o aplicativo
echo 📦 Publicando aplicativo...
dotnet publish "GamingTweaksManager.csproj" --configuration Release --output "./bin/Release/Publish" --self-contained false
if %errorlevel% neq 0 (
    echo ❌ Erro na publicação
    pause
    exit /b 1
)

echo ✅ Aplicativo publicado com sucesso!
echo.
echo 📁 Arquivos disponíveis em: ./bin/Release/Publish/
echo.
echo ⚠️  IMPORTANTE: Execute o aplicativo como Administrador para aplicar os tweaks!
echo.
pause

@echo off
setlocal

set FRONTEND_PATH=PhishGuard.Frontend
set WEBAPI_PATH=PhishGuard.Backend\PhishGuard.WebAPI

echo ======================================================
echo 💻 SUBINDO AMBIENTE LOCAL (HOT-RELOAD ACTIVATED)
echo ======================================================

echo [1/4] Garantindo que apenas o Banco de Dados (Postgres) esta de pe...
docker compose up -d postgres

echo [2/4] Matando processos antigos travados no Windows...
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM node.exe >nul 2>&1

timeout /t 1 >nul

echo [3/4] Iniciando o Backend da API em uma nova janela...
start "PhishGuard WebAPI" cmd /k "cd /d %WEBAPI_PATH% && dotnet watch run"

echo [4/4] Iniciando o Frontend em uma nova janela...
start "PhishGuard Frontend" cmd /k "cd /d %FRONTEND_PATH% && npm run dev"

echo 🎉 Janelas independentes abertas com sucesso!
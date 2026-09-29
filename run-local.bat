@echo off
setlocal
cd /d "%~dp0"

set FRONTEND_PATH=PhishGuard.Frontend
set WEBAPI_PATH=PhishGuard.Backend

echo ======================================================
echo INICIANDO AMBIENTE LOCAL COM HOT-RELOAD
echo ======================================================

if not exist ".env" (
    echo ERRO: arquivo .env nao encontrado.
    echo Copie .env.example para .env e preencha os segredos.
    exit /b 1
)

rem Carrega somente as chaves conhecidas. O .env nunca e executado como script e
rem nenhum segredo e escrito no console.
for /f "usebackq eol=# tokens=1,* delims==" %%A in (".env") do (
    if /I "%%A"=="POSTGRES_DB" set "POSTGRES_DB=%%B"
    if /I "%%A"=="POSTGRES_USER" set "POSTGRES_USER=%%B"
    if /I "%%A"=="POSTGRES_PASSWORD" set "POSTGRES_PASSWORD=%%B"
    if /I "%%A"=="POSTGRES_PORT" set "POSTGRES_PORT=%%B"
    if /I "%%A"=="AppSettings__Token" set "AppSettings__Token=%%B"
    if /I "%%A"=="NGROK_DOMAIN" set "NGROK_DOMAIN=%%B"
)

if not defined POSTGRES_DB goto config_error
if not defined POSTGRES_USER goto config_error
if not defined POSTGRES_PASSWORD goto config_error
if not defined POSTGRES_PORT set "POSTGRES_PORT=5433"
if not defined AppSettings__Token goto config_error

set "ConnectionStrings__DefaultConnection=Host=localhost;Port=%POSTGRES_PORT%;Database=%POSTGRES_DB%;Username=%POSTGRES_USER%;Password=%POSTGRES_PASSWORD%"

echo [1/5] Validando ferramentas obrigatorias...
docker --version >nul 2>&1 || goto docker_error
dotnet --version >nul 2>&1 || goto dotnet_error
node --version >nul 2>&1 || goto node_error

echo [2/5] Subindo o PostgreSQL e aguardando o healthcheck...
docker compose up -d --wait db
if errorlevel 1 (
    echo ERRO: o PostgreSQL nao ficou saudavel. Confira o Docker Desktop e execute:
    echo       docker compose logs db
    exit /b 1
)

echo [3/5] Encerrando somente processos antigos nas portas 5000 e 5173...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$processIds = Get-NetTCPConnection -State Listen -LocalPort 5000,5173 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique; $processIds | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }"

powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Sleep -Seconds 1"

echo [4/5] Iniciando o Backend da API em uma nova janela...
start "PhishGuard WebAPI" cmd /k "cd /d %WEBAPI_PATH% && dotnet watch run"

echo [5/5] Iniciando o Frontend em uma nova janela...
start "PhishGuard Frontend" cmd /k "cd /d %FRONTEND_PATH% && npm run dev"

echo Aguardando Backend (5000) e Frontend (5173), por ate 60 segundos...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$deadline = (Get-Date).AddSeconds(60); do { $backend = Get-NetTCPConnection -State Listen -LocalPort 5000 -ErrorAction SilentlyContinue; $frontend = Get-NetTCPConnection -State Listen -LocalPort 5173 -ErrorAction SilentlyContinue; if ($backend -and $frontend) { exit 0 }; Start-Sleep -Seconds 1 } while ((Get-Date) -lt $deadline); exit 1"
if errorlevel 1 (
    echo ERRO: um dos servicos nao iniciou. Consulte as janelas PhishGuard WebAPI e PhishGuard Frontend.
    exit /b 1
)

echo Ambiente local pronto!
echo API:      http://localhost:5000/swagger
echo Frontend: http://localhost:5173
exit /b 0

:config_error
echo ERRO: .env incompleto. Preencha POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD e AppSettings__Token.
exit /b 1

:docker_error
echo ERRO: Docker nao encontrado ou Docker Desktop indisponivel.
exit /b 1

:dotnet_error
echo ERRO: .NET SDK 8 nao encontrado.
exit /b 1

:node_error
echo ERRO: Node.js nao encontrado.
exit /b 1

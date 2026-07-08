@echo off
setlocal

echo ======================================================
echo  RODANDO SUITE DE TESTES DO PHISHGUARD
echo ======================================================

set "EXIT_CODE=0"

echo.
echo [1/2] Backend - xUnit / .NET (EF InMemory, nao precisa de banco)...
dotnet test PhishGuard.Tests\PhishGuard.Tests.csproj --nologo
if errorlevel 1 set "EXIT_CODE=1"

echo.
echo [2/2] Frontend - Vitest (testes de UI em PhishGuard.Tests\Frontend)...
pushd PhishGuard.Frontend
call npm run test
if errorlevel 1 set "EXIT_CODE=1"
popd

echo.
echo ======================================================
if "%EXIT_CODE%"=="0" echo  RESULTADO: TODOS OS TESTES PASSARAM.
if not "%EXIT_CODE%"=="0" echo  RESULTADO: ALGUM CONJUNTO DE TESTES FALHOU.
echo ======================================================

exit /b %EXIT_CODE%

param([ValidateRange(1024, 65535)][int]$Port = 55433)

$ErrorActionPreference = 'Stop'
$containerName = 'phishguard-mvp-tests-' + [guid]::NewGuid().ToString('N')
$testPassword = [guid]::NewGuid().ToString('N')
$originalConnection = $env:PHISHGUARD_TEST_POSTGRES
$created = $false
try {
    docker run -d --name $containerName -e "POSTGRES_PASSWORD=$testPassword" `
        -e POSTGRES_DB=phishguard_mvp_tests -p "127.0.0.1:${Port}:5433" postgres:16-alpine -p 5433 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Não foi possível criar o PostgreSQL descartável.' }
    $created = $true
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        docker exec $containerName pg_isready -U postgres -d phishguard_mvp_tests -p 5433 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $ready) { throw 'O PostgreSQL de teste não ficou pronto.' }
    $env:PHISHGUARD_TEST_POSTGRES = "Host=127.0.0.1;Port=$Port;Database=phishguard_mvp_tests;Username=postgres;Password=$testPassword;Include Error Detail=false"
    dotnet test (Join-Path $PSScriptRoot 'PhishGuard.Tests/PhishGuard.Tests.csproj') --no-restore
    $testExitCode = $LASTEXITCODE
}
finally {
    $env:PHISHGUARD_TEST_POSTGRES = $originalConnection
    if ($created) { docker rm -f -v $containerName | Out-Null }
}
exit $testExitCode

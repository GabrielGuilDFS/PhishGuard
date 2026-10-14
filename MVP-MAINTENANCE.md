# Diagnóstico e purga de tenant

Estas ferramentas são operações administrativas de manutenção. Não são endpoints do produto e não são disparadas pelo startup. O diagnóstico foi executado no banco local em 03/10/2026 durante a manutenção autorizada, sem conflitos. A purga não foi executada no banco de usuários.

Backup e upgrade locais concluídos em `.backups/mvp-before-migration-20261003-005642-e5ed08/`, com restauração isolada comprovada, preservação dos registros originais e recuperação documentada. O backend atualizado foi retomado saudável após aplicar as duas migrations.

## Antes de aplicar a nova migration

1. Fazer backup e comprovar sua restauração em uma instalação de teste.
2. Executar `scripts/check-mvp-integrity.sql` em uma conexão administrativa autorizada. O script usa uma transação somente leitura e retorna duplicidades, IDs com referências ausentes/cross-tenant e logs/entregas cujo alvo não participa da campanha.
3. Revisar cada conflito. Não apagar histórico nem inventar associações para satisfazer uma constraint. Se a associação estiver comprovada por outra fonte, corrigir o registro com um procedimento específico e revisado; caso contrário, preservar os dados e interromper o upgrade.
4. Aplicar a migration em cópia restaurada e conferir os resultados antes do upgrade real. As constraints compostas e os índices têm custo de validação e bloqueio proporcional à base.

O diagnóstico funciona também no esquema anterior, em que `campaign_target` ainda não tinha `tenant_id`. Usa o tenant da campanha para verificar o par campanha/alvo. Não reconstrói o departamento histórico: snapshots antigos preenchidos com o cadastro atual são marcados como legado.

## Purga completa e explícita

`scripts/purge-tenant.sql` exige um UUID de tenant. Por padrão, executa as exclusões dentro de uma transação e termina com **ROLLBACK**, preservando os dados. Um erro aborta a execução antes do commit.

Antes da purga definitiva, fazer backup, parar a API e todos os workers e confirmar a identidade do tenant. O lock do registro do tenant coordena escritores HTTP, mas não cancela um envio SMTP/API externa que já esteja em andamento. Não executar com o serviço de disparo ativo.

Simulação, com a conexão configurada para o banco de manutenção:

```text
psql --dbname "<conexão autorizada>" --set tenant_id="<UUID do tenant>" --file scripts/purge-tenant.sql
```

Depois de revisar a simulação, a aplicação definitiva exige a opção explícita:

```text
psql --dbname "<conexão autorizada>" --set tenant_id="<UUID do tenant>" --set apply=true --file scripts/purge-tenant.sql
```

As variáveis `:'tenant_id'` são escapadas pelo psql como literais e convertidas para `uuid`; não compor comandos SQL com concatenação. Usar o gerenciamento de credenciais do cliente para evitar expor senhas no histórico de comandos.

A ordem é logs → entregas → participações → campanhas → sessões → administradores → SMTP → alvos → modelos/páginas → tenant. Todo DELETE tem o mesmo escopo explícito de tenant. Chaves globais de Data Protection ficam preservadas porque podem ser necessárias para outros tenants.

## Verificação automatizada

`PhishGuard.Tests/Data/TenantRelationalIntegrityTests.cs` executa os DELETEs extraídos do script real em SQLite e em PostgreSQL descartável. Verifica rollback, commit, preservação completa de um segundo tenant e manutenção das chaves globais. Também tenta gravar relações cross-tenant diretamente via SQL parametrizado, registrar histórico de um alvo não participante e remover uma participação com histórico.

```text
dotnet test PhishGuard.Tests/PhishGuard.Tests.csproj --filter FullyQualifiedName~TenantRelationalIntegrityTests
```

Os testes PostgreSQL dependem de `PHISHGUARD_TEST_POSTGRES` apontando para a instância descartável gerenciada por `run-mvp-integration-tests.ps1`. SQLite testa constraints portáveis; o PostgreSQL confirma o SQLSTATE e a migration real.

using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;

namespace PhishGuard.Tests.Data;

// Provedor de tenant MUTÁVEL: permite "virar" o tenant ativo no MESMO contexto entre
// consultas — o Global Query Filter (`x.TenantId == this.TenantIdAtual`) é reavaliado a
// cada query, então trocar o Id aqui muda o que o contexto enxerga.
public sealed class MutableTenantProvider : ITenantProvider
{
    public Guid TenantIdAtivo { get; set; }
    public Guid GetTenantId() => TenantIdAtivo;
    public Guid GetCurrentTenantId() => TenantIdAtivo;
}

// Harness da suíte de isolamento de tenant (Passo 1). Banco SQLite IN-MEMORY: relacional
// DE VERDADE (impõe FKs, unique e a tradução real de query), diferente do provedor
// InMemory do EF Core. O banco vive enquanto a conexão estiver ABERTA — por isso a
// conexão é mantida aberta e fechada só no Dispose. Uma instância POR TESTE => cada teste
// tem um banco limpo e isolado (sem estado compartilhado entre testes).
public sealed class SqliteTenantHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    public MutableTenantProvider Tenant { get; }
    public AppDbContext Db { get; }

    public SqliteTenantHarness(Guid tenantInicial)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open(); // mantém o banco in-memory vivo por toda a vida do harness

        Tenant = new MutableTenantProvider { TenantIdAtivo = tenantInicial };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new AppDbContext(options, Tenant);
        // Cria o schema a partir do MODELO real do AppDbContext (com FKs, unique e o
        // gate provider-aware do xmin). Prova, de quebra, que o modelo é criável fora do Npgsql.
        Db.Database.EnsureCreated();
    }

    /// <summary>Troca o tenant ativo (para simular outra sessão no mesmo contexto).</summary>
    public void UsarTenant(Guid tenantId) => Tenant.TenantIdAtivo = tenantId;

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose(); // fecha a conexão => destrói o banco in-memory
    }
}

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.Security;

namespace PhishGuard.Backend.Data
{
    /// <summary>
    /// Factory de design-time usada EXCLUSIVAMENTE pela ferramenta do EF Core
    /// (dotnet ef migrations/database). Ao existir, o EF a usa em vez de subir o
    /// Program.cs — evitando disparar a automigração e o worker durante a geração de
    /// migrations, e dando uma construção controlada do AppDbContext (que exige um
    /// ITenantProvider). Em runtime normal, o DI continua criando o contexto.
    /// </summary>
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5433;Database=phishguard_db;Username=postgres;Password=new";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AppDbContext(options, new DesignTimeTenantProvider());
        }

        // Sem contexto de requisição no design-time: tenant "vazio". As migrations
        // dependem apenas do modelo (schema), não de dados por tenant.
        private sealed class DesignTimeTenantProvider : ITenantProvider
        {
            public Guid GetTenantId() => Guid.Empty;
            public Guid GetCurrentTenantId() => Guid.Empty;
        }
    }
}

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Security;
 
namespace PhishGuard.Backend.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        private readonly ITenantProvider _tenantProvider;
        private Guid? _systemTenantScopeId;

        public AppDbContext(
            DbContextOptions<AppDbContext> options, 
            ITenantProvider tenantProvider,
            AuthSessionCache? sessionCache = null) : base(options)
        { 
            _tenantProvider = tenantProvider;
            if (sessionCache is not null)
            {
                var authenticationWrite = false;
                SavingChanges += (_, _) =>
                {
                    authenticationWrite = ChangeTracker.Entries().Any(e =>
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && e.Entity is AuthSession or Administrador or Tenant);
                    if (authenticationWrite)
                        sessionCache.Invalidate(Database.CurrentTransaction is not null
                            || System.Transactions.Transaction.Current is not null);
                };
                SavedChanges += (_, _) => { if (authenticationWrite) sessionCache.Invalidate(); };
                SaveChangesFailed += (_, _) => { if (authenticationWrite) sessionCache.Invalidate(); };
            }
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Target> Targets { get; set; }
        public DbSet<SmtpConfig> SmtpConfigs { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<PhishingPage> PhishingPages { get; set; }
        public DbSet<EducationalPage> EducationalPages { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignTarget> CampaignTargets { get; set; }
        public DbSet<SimulationLog> SimulationsLogs { get; set; }
        public DbSet<CampaignDelivery> CampaignDeliveries { get; set; }
        public DbSet<AuthSession> AuthSessions { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public Guid TenantIdAtual => _systemTenantScopeId ?? _tenantProvider.GetTenantId();

        // A signed public campaign link has no JWT tenant. Its capability service opens
        // a narrow, disposable tenant scope so even anonymous reads retain query filters.
        internal IDisposable UseSystemTenantScope(Guid tenantId)
        {
            var requestTenant = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty || (requestTenant != Guid.Empty && requestTenant != tenantId)
                || (_systemTenantScopeId.HasValue && _systemTenantScopeId.Value != tenantId))
                throw new InvalidOperationException("Escopo de tenant inválido.");
            var previous = _systemTenantScopeId;
            _systemTenantScopeId = tenantId;
            return new TenantScope(this, previous);
        }

        private sealed class TenantScope(AppDbContext context, Guid? previous) : IDisposable
        {
            private bool _disposed;
            public void Dispose()
            {
                if (_disposed) return;
                context._systemTenantScopeId = previous;
                _disposed = true;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NomeEmpresa)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Cnpj)
                    .IsRequired()
                    .HasMaxLength(14);

                entity.Property(e => e.Ativo)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CriadoEm)
                    .IsRequired();

                entity.Property(e => e.Plano)
                    .IsRequired()
                    .HasDefaultValue(PlanoTenant.Bronze);

                entity.HasIndex(e => e.Cnpj)
                    .IsUnique();
            });
            
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Administrador>(entity =>
            {
                entity.HasAlternateKey(a => new { a.TenantId, a.Id });
                entity.HasOne<Tenant>().WithMany(t => t.Administradores).HasForeignKey(a => a.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(a => a.TenantId == this.TenantIdAtual);

                entity.Property(e => e.Nome)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);

                // Anti-brute-force: contador de falhas consecutivas (default 0 nas linhas
                // existentes) e janela de bloqueio temporário (nula = liberada).
                entity.Property(e => e.AcessoFalhasContador)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.BloqueioFim)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Target>(entity =>
            {
                entity.ToTable("Alvos"); 
                entity.HasAlternateKey(a => new { a.TenantId, a.Id });
                entity.HasOne<Tenant>().WithMany(t => t.Alvos).HasForeignKey(a => a.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(a => a.TenantId == this.TenantIdAtual);

                entity.Property(e => e.Nome)
                    .IsRequired()
                    .HasMaxLength(150); 

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150); 

                entity.Property(e => e.EmailNormalizado).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => new { e.TenantId, e.EmailNormalizado })
                    .IsUnique().HasDatabaseName("ux_alvos_tenant_email_normalizado");

                entity.Property(e => e.Departamento)
                    .IsRequired()
                    .HasMaxLength(80); 
            });

            modelBuilder.Entity<Campaign>().HasMany(c => c.Targets).WithMany(t => t.Campaigns)
                .UsingEntity<CampaignTarget>(
                    right => right.HasOne(p => p.Target).WithMany(t => t.CampaignTargets)
                        .HasForeignKey(p => new { p.TenantId, p.TargetsId })
                        .HasPrincipalKey(t => new { t.TenantId, t.Id })
                        .OnDelete(DeleteBehavior.Restrict),
                    left => left.HasOne(p => p.Campaign).WithMany(c => c.CampaignTargets)
                        .HasForeignKey(p => new { p.TenantId, p.CampaignsId })
                        .HasPrincipalKey(c => new { c.TenantId, c.Id })
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("campaign_target");
                        join.HasKey(p => new { p.CampaignsId, p.TargetsId });
                        join.HasAlternateKey(p => new { p.TenantId, p.CampaignsId, p.TargetsId });
                        join.HasQueryFilter(p => p.TenantId == this.TenantIdAtual);
                    });

            modelBuilder.Entity<SmtpConfig>(entity =>
            {
                entity.ToTable("smtp_configs");

                entity.HasQueryFilter(e => e.TenantId == this.TenantIdAtual);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Host)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Porta)
                    .IsRequired();

                entity.Property(e => e.Usuario)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Senha)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.Property(e => e.SenderEmail)
                    .IsRequired()
                    .HasMaxLength(254);

                entity.Property(e => e.SenderName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.EncryptedApiKey)
                    .IsRequired()
                    .HasMaxLength(4096);

                entity.Property(e => e.ApiAccountIdentifier)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ApiRegion)
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasDefaultValue("us-east-1");

                entity.Property(e => e.UltimoErroCodigo)
                    .HasMaxLength(50);

                entity.HasOne(e => e.Tenant)
                    .WithOne(t => t.SmtpConfig)
                    .HasForeignKey<SmtpConfig>(e => e.TenantId)
                    .IsRequired();
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.ToTable("Templates");
                entity.HasAlternateKey(t => new { t.TenantId, t.Id });

                entity.HasQueryFilter(t => t.TenantId == this.TenantIdAtual);

                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Assunto).IsRequired().HasMaxLength(150);
                entity.Property(e => e.RemetenteNome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RemetenteEmail).IsRequired().HasMaxLength(150);
                entity.Property(e => e.CorpoHtml).IsRequired();

                // FK física TenantId → Tenants (integridade referencial no banco, além do
                // Global Query Filter da aplicação). Cascade: purgar o tenant remove seus dados.
                entity.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PhishingPage>(entity =>
            {
                entity.ToTable("PhishingPages");
                entity.HasAlternateKey(p => new { p.TenantId, p.Id });

                entity.HasQueryFilter(p => p.TenantId == this.TenantIdAtual);

                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HtmlCaptura).IsRequired();

                // FK física TenantId → Tenants.
                entity.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EducationalPage>(entity =>
            {
                entity.ToTable("EducationalPages");
                entity.HasAlternateKey(p => new { p.TenantId, p.Id });

                entity.HasQueryFilter(e => e.TenantId == this.TenantIdAtual);

                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HtmlEducacional).IsRequired();

                // FK física TenantId → Tenants.
                entity.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.ToTable("Campaigns");
                entity.HasAlternateKey(c => new { c.TenantId, c.Id });

                entity.HasQueryFilter(c => c.TenantId == this.TenantIdAtual);

                entity.Property(c => c.DispatchErrorCode).HasMaxLength(50);
                entity.Property(c => c.DispatchErrorMessage).HasMaxLength(300);
                entity.Property(c => c.DispatchAttemptCount).HasDefaultValue(0);

                // Claim ATÔMICO para scale-out: usa a coluna de sistema 'xmin' do Postgres
                // como token de concorrência otimista (sem coluna/migration extra). O UPDATE
                // do claim (Agendada → Processando) passa a incluir "WHERE xmin = @original";
                // se duas instâncias do worker reivindicarem a MESMA campanha, apenas a
                // primeira efetiva — a segunda recebe DbUpdateConcurrencyException e desiste,
                // impedindo o disparo duplicado do lote.
                //
                // 'xmin' é uma coluna de SISTEMA exclusiva do PostgreSQL: só faz sentido (e só
                // mapeia) sob o Npgsql. Por isso é aplicada de forma provider-aware — sob SQLite
                // (suíte de isolamento de tenant) ou InMemory a anotação seria metadado inócuo,
                // mas o EnsureCreated() do SQLite tentaria materializar a coluna e falharia. O
                // gate mantém o modelo de PRODUÇÃO idêntico e o de teste criável.
                //
                // Mapeamento EXPLÍCITO (shadow property) em vez do atalho
                // 'UseXminAsConcurrencyToken()' do Npgsql: aquele método foi marcado obsoleto e
                // depois REMOVIDO nas versões novas do provider — usá-lo quebrava o build quando
                // o restore unificava o Npgsql para uma versão >8. Esta forma é 100% EF Core
                // padrão (nenhuma extensão Npgsql-específica), compila em qualquer versão do
                // provider e expande para EXATAMENTE o que o atalho gerava, então o
                // ModelSnapshot continua idêntico (sem migration nova).
                if (Database.IsNpgsql())
                    entity.Property<uint>("xmin")
                        .HasColumnName("xmin")
                        .HasColumnType("xid")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();

                // FK física TenantId → Tenants (Cascade: purga do tenant).
                entity.HasOne<Tenant>().WithMany().HasForeignKey(c => c.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                // INTEGRIDADE HISTÓRICA (auditoria): apagar um modelo de e-mail ou uma
                // página (falsa/educativa) NÃO pode derrubar campanhas antigas em cascata —
                // isso destruiria o histórico de relatórios. RESTRICT bloqueia a exclusão do
                // recurso enquanto houver campanha que o referencie.
                entity.HasOne(c => c.Template).WithMany()
                    .HasForeignKey(c => new { c.TenantId, c.EmailTemplateId })
                    .HasPrincipalKey(t => new { t.TenantId, t.Id })
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.PhishingPage).WithMany()
                    .HasForeignKey(c => new { c.TenantId, c.LandingPageId })
                    .HasPrincipalKey(p => new { p.TenantId, p.Id })
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.EducationalPage).WithMany()
                    .HasForeignKey(c => new { c.TenantId, c.EducationalPageId })
                    .HasPrincipalKey(p => new { p.TenantId, p.Id })
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SimulationLog>(entity =>
            {
                // Global Query Filter (isolamento de tenant): SimulationLog tem TenantId, logo
                // — pela regra do projeto — PRECISA do filtro, como toda entidade multi-tenant.
                // Dashboards e tracking mantêm filtros e escopo explícito; somente o worker
                // desabilita filtros ao executar operações de sistema reescopadas por tenant.
                // O filtro é defesa-em-profundidade
                // contra QUALQUER consulta futura que esqueça o escopo e vaze logs cross-tenant.
                entity.HasQueryFilter(l => l.TenantId == this.TenantIdAtual);

                // simulations_logs já usa snake_case via data annotations ([Table]/[Column]).
                // Compound keys also enforce tenant isolation for non-HTTP writers.
                entity.HasOne<Campaign>().WithMany()
                    .HasForeignKey(l => new { l.TenantId, l.CampaignId })
                    .HasPrincipalKey(c => new { c.TenantId, c.Id })
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Target>().WithMany()
                    .HasForeignKey(l => new { l.TenantId, l.TargetId })
                    .HasPrincipalKey(t => new { t.TenantId, t.Id })
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<CampaignTarget>().WithMany()
                    .HasForeignKey(l => new { l.TenantId, l.CampaignId, l.TargetId })
                    .HasPrincipalKey(p => new { p.TenantId, p.CampaignsId, p.TargetsId })
                    .HasConstraintName("fk_simulations_logs_participant")
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Tenant>().WithMany().HasForeignKey(l => l.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Dashboard executivo: o prefixo (tenant_id, data_hora) restringe a
                // janela temporal antes de classificar a ação e deduplicar o par
                // campanha+alvo. Todos os membros têm o mesmo tipo físico usado pelos
                // predicados EF (uuid/timestamptz-via-timestamp/text/uuid/uuid).
                entity.HasIndex(l => new
                    {
                        l.TenantId,
                        l.DataHora,
                        l.Acao,
                        l.CampaignId,
                        l.TargetId
                    })
                    .HasDatabaseName("ix_simulations_logs_dashboard_overview");

                // Idempotência concorrente: a checagem no TrackingController evita a
                // escrita comum, e este índice fecha a janela de corrida entre duas
                // requisições simultâneas do mesmo evento.
                entity.HasIndex(l => new { l.CampaignId, l.TargetId, l.Acao })
                    .IsUnique()
                    .HasDatabaseName("ux_simulations_logs_campaign_target_action");
            });

            modelBuilder.Entity<CampaignDelivery>(entity =>
            {
                entity.HasQueryFilter(delivery => delivery.TenantId == this.TenantIdAtual);

                entity.Property(delivery => delivery.IdempotencyKey)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(delivery => delivery.ProviderMessageId).HasMaxLength(200);
                entity.Property(delivery => delivery.LastErrorCode).HasMaxLength(100);
                entity.Property(delivery => delivery.AttemptCount).HasDefaultValue(0);
                entity.Property(delivery => delivery.ConcurrencyToken).IsConcurrencyToken();

                entity.HasIndex(delivery => new { delivery.CampaignId, delivery.TargetId })
                    .IsUnique()
                    .HasDatabaseName("ux_campaign_deliveries_campaign_target");
                entity.HasIndex(delivery => new { delivery.TenantId, delivery.Status, delivery.LeaseExpiresAtUtc })
                    .HasDatabaseName("ix_campaign_deliveries_tenant_status_lease");

                entity.HasOne(delivery => delivery.Campaign)
                    .WithMany()
                    .HasForeignKey(delivery => new { delivery.TenantId, delivery.CampaignId })
                    .HasPrincipalKey(c => new { c.TenantId, c.Id })
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(delivery => delivery.Target)
                    .WithMany()
                    .HasForeignKey(delivery => new { delivery.TenantId, delivery.TargetId })
                    .HasPrincipalKey(t => new { t.TenantId, t.Id })
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<CampaignTarget>().WithMany()
                    .HasForeignKey(delivery => new { delivery.TenantId, delivery.CampaignId, delivery.TargetId })
                    .HasPrincipalKey(p => new { p.TenantId, p.CampaignsId, p.TargetsId })
                    .HasConstraintName("fk_campaign_deliveries_participant")
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Tenant>()
                    .WithMany()
                    .HasForeignKey(delivery => delivery.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuthSession>(entity =>
            {
                entity.HasKey(session => session.Id);
                entity.HasQueryFilter(session => session.TenantId == this.TenantIdAtual);
                entity.Property(session => session.RefreshTokenHash).IsRequired().HasMaxLength(64);
                entity.HasIndex(session => session.RefreshTokenHash).IsUnique();
                entity.HasIndex(session => new { session.TenantId, session.AdministratorId, session.RevokedAtUtc });
                entity.HasOne<Administrador>()
                    .WithMany()
                    .HasForeignKey(session => new { session.TenantId, session.AdministratorId })
                    .HasPrincipalKey(a => new { a.TenantId, a.Id })
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Tenant>()
                    .WithMany()
                    .HasForeignKey(session => session.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------------------------------------------------------------------
            // PADRONIZAÇÃO snake_case (nativo do PostgreSQL). Executa por ÚLTIMO, sobre o
            // modelo já configurado: renomeia tabelas, colunas, PKs, FKs e índices para
            // snake_case. Como só afeta os nomes FÍSICOS, o C# segue em PascalCase e as
            // queries LINQ (sobre propriedades CLR) continuam idênticas. Idempotente para
            // nomes que já são snake_case (ex.: 'simulations_logs', 'campaign_id').
            // ---------------------------------------------------------------------------
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                    entityType.SetTableName(ToSnakeCase(tableName));

                foreach (var property in entityType.GetProperties())
                {
                    var columnName = property.GetColumnName();
                    if (!string.IsNullOrEmpty(columnName))
                        property.SetColumnName(ToSnakeCase(columnName));
                }

                foreach (var key in entityType.GetKeys())
                {
                    var keyName = key.GetName();
                    if (!string.IsNullOrEmpty(keyName))
                        key.SetName(ToSnakeCase(keyName));
                }

                foreach (var fk in entityType.GetForeignKeys())
                {
                    var fkName = fk.GetConstraintName();
                    if (!string.IsNullOrEmpty(fkName))
                        fk.SetConstraintName(ToSnakeCase(fkName));
                }

                foreach (var index in entityType.GetIndexes())
                {
                    var indexName = index.GetDatabaseName();
                    if (!string.IsNullOrEmpty(indexName))
                        index.SetDatabaseName(ToSnakeCase(indexName));
                }
            }
        }

        // Converte PascalCase/camelCase para snake_case, preservando nomes que já estão em
        // snake_case (idempotente) e tratando siglas coladas (ex.: 'IpOrigem' → 'ip_origem',
        // 'FK_Campaigns_Templates_EmailTemplateId' → 'fk_campaigns_templates_email_template_id').
        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var builder = new StringBuilder(name.Length + 8);
            for (int i = 0; i < name.Length; i++)
            {
                char current = name[i];
                if (char.IsUpper(current))
                {
                    bool previousIsSeparator = i > 0 && name[i - 1] == '_';
                    bool boundary = i > 0 && !previousIsSeparator &&
                        (!char.IsUpper(name[i - 1]) ||
                         (i + 1 < name.Length && char.IsLower(name[i + 1])));
                    if (boundary)
                        builder.Append('_');
                    builder.Append(char.ToLowerInvariant(current));
                }
                else
                {
                    builder.Append(current);
                }
            }
            return builder.ToString();
        }


        private void StampCurrentTenant()
        {
            foreach (var entry in ChangeTracker.Entries<Target>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified))
                entry.Entity.EmailNormalizado = Target.NormalizeEmail(entry.Entity.Email);

            var currentTenantId = this.TenantIdAtual; 
            if (currentTenantId != Guid.Empty)
            {
                foreach (var entry in ChangeTracker.Entries())
                {
                    if (entry.State == EntityState.Added && entry.Entity.GetType().GetProperty("TenantId") != null)
                    {
                        entry.Property("TenantId").CurrentValue = currentTenantId;
                    }
                }
            }

            // The association is also created by EF's skip navigation. System contexts
            // have no request tenant, so derive its scope from the owning campaign.
            foreach (var entry in ChangeTracker.Entries<CampaignTarget>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty))
            {
                if (entry.Entity.Campaign is not null)
                    entry.Entity.TenantId = entry.Entity.Campaign.TenantId;
            }

            ProtectCapturedSnapshots();

        }

        private void ProtectCapturedSnapshots()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified && e.Entity is Campaign or CampaignTarget))
            {
                if (entry.Property("SnapshotCapturedAtUtc").OriginalValue is null)
                    continue;

                // Status and delivery bookkeeping may change; captured identity/content may not.
                if (entry.Properties.Any(p => p.IsModified &&
                    (p.Metadata.Name.EndsWith("Snapshot", StringComparison.Ordinal)
                     || p.Metadata.Name == "SnapshotCapturedAtUtc")))
                    throw new InvalidOperationException("O snapshot de uma campanha ativada é imutável.");
            }
        }

        public override int SaveChanges()
        {
            StampCurrentTenant();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            StampCurrentTenant();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampCurrentTenant();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            StampCurrentTenant();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Models;
 
namespace PhishGuard.Backend.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;

        public AppDbContext(
            DbContextOptions<AppDbContext> options, 
            ITenantProvider tenantProvider) : base(options) 
        { 
            _tenantProvider = tenantProvider;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Target> Targets { get; set; }
        public DbSet<SmtpConfig> SmtpConfigs { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<PhishingPage> PhishingPages { get; set; }
        public DbSet<EducationalPage> EducationalPages { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<SimulationLog> SimulationsLogs { get; set; }

        public Guid TenantIdAtual => _tenantProvider.GetTenantId();

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

                entity.HasQueryFilter(a => a.TenantId == this.TenantIdAtual);

                entity.Property(e => e.Nome)
                    .IsRequired()
                    .HasMaxLength(150); 

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150); 

                entity.Property(e => e.Departamento)
                    .IsRequired()
                    .HasMaxLength(80); 
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
                    .HasMaxLength(255);

                entity.HasOne(e => e.Tenant)
                    .WithOne(t => t.SmtpConfig)
                    .HasForeignKey<SmtpConfig>(e => e.TenantId)
                    .IsRequired();
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.ToTable("Templates");

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

                entity.HasQueryFilter(c => c.TenantId == this.TenantIdAtual);

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
                entity.HasOne(c => c.Template).WithMany().HasForeignKey(c => c.EmailTemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.PhishingPage).WithMany().HasForeignKey(c => c.LandingPageId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.EducationalPage).WithMany().HasForeignKey(c => c.EducationalPageId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SimulationLog>(entity =>
            {
                // Global Query Filter (isolamento de tenant): SimulationLog tem TenantId, logo
                // — pela regra do projeto — PRECISA do filtro, como toda entidade multi-tenant.
                // Todos os leitores atuais já escopam manualmente (Dashboard: Where TenantId;
                // Tracking e Dispatch: IgnoreQueryFilters), então o filtro é defesa-em-profundidade
                // contra QUALQUER consulta futura que esqueça o escopo e vaze logs cross-tenant.
                entity.HasQueryFilter(l => l.TenantId == this.TenantIdAtual);

                // simulations_logs já usa snake_case via data annotations ([Table]/[Column]).
                // Aqui declaramos as FKs FÍSICAS que faltavam — sem elas, os logs eram GUIDs
                // soltos (risco de registro órfão). Cascade: se a campanha/alvo/tenant for
                // removido, os logs de auditoria correspondentes vão junto (sem órfãos).
                entity.HasOne<Campaign>().WithMany().HasForeignKey(l => l.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Target>().WithMany().HasForeignKey(l => l.TargetId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Tenant>().WithMany().HasForeignKey(l => l.TenantId)
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


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
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

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
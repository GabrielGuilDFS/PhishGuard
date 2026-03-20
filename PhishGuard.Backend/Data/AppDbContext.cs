using System;
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

                entity.Property(e => e.SenhaHash)
                    .IsRequired()
                    .HasMaxLength(255); 
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
            });
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
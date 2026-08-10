using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Services;

public interface IAdminBootstrapProvisioner
{
    Task ProvisionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Provisiona uma única conta administrativa de bootstrap a partir de variáveis de
/// ambiente. O fluxo é idempotente, não redefine senhas existentes e nunca registra
/// credenciais em log. Deve ser desabilitado após o primeiro deploy bem-sucedido.
/// </summary>
public sealed class AdminBootstrapProvisioner : IAdminBootstrapProvisioner
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminBootstrapProvisioner> _logger;

    public AdminBootstrapProvisioner(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<AdminBootstrapProvisioner> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProvisionAsync(CancellationToken cancellationToken = default)
    {
        if (!_configuration.GetValue<bool>("BootstrapAdmin:Enabled"))
            return;

        var input = ReadAndValidateConfiguration();
        var existingTenant = await _context.Tenants
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(tenant => tenant.Cnpj == input.Cnpj, cancellationToken);
        var existingAdmin = await _context.Administradores
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(admin => admin.Email == input.Email, cancellationToken);

        if (existingTenant is not null || existingAdmin is not null)
        {
            if (existingTenant is not null
                && existingAdmin is not null
                && existingAdmin.TenantId == existingTenant.Id)
            {
                _logger.LogInformation(
                    "Bootstrap administrativo já provisionado para o tenant {TenantId}; nenhuma alteração foi realizada.",
                    existingTenant.Id);
                return;
            }

            throw new InvalidOperationException(
                "Bootstrap administrativo em conflito: o CNPJ ou e-mail já pertence a outro cadastro.");
        }

        IDbContextTransaction? transaction = null;
        if (_context.Database.IsRelational())
            transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await using (transaction)
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                NomeEmpresa = input.TenantName,
                Cnpj = input.Cnpj,
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                Plano = input.Plan
            };
            var administrator = new Administrador
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Nome = input.AdministratorName,
                Email = input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
                AcessoFalhasContador = 0,
                BloqueioFim = null
            };

            _context.Tenants.Add(tenant);
            _context.Administradores.Add(administrator);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                if (transaction is not null)
                    await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                if (transaction is not null)
                    await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(
                    "Não foi possível provisionar o administrador de bootstrap por conflito de dados.",
                    ex);
            }

            _logger.LogInformation(
                "Administrador de bootstrap {AdministratorId} provisionado para o tenant {TenantId}.",
                administrator.Id,
                tenant.Id);
        }
    }

    private BootstrapInput ReadAndValidateConfiguration()
    {
        var tenantName = Required("BootstrapAdmin:TenantName", maxLength: 150);
        var cnpj = new string(Required("BootstrapAdmin:Cnpj", maxLength: 32)
            .Where(character => character is >= '0' and <= '9')
            .ToArray());
        var administratorName = Required("BootstrapAdmin:Name", maxLength: 150);
        var email = Required("BootstrapAdmin:Email", maxLength: 150).ToLowerInvariant();
        var password = Required("BootstrapAdmin:Password", maxLength: 100, trim: false);
        var planRaw = _configuration["BootstrapAdmin:Plan"]?.Trim();

        if (cnpj.Length != 14)
            throw new InvalidOperationException("BootstrapAdmin:Cnpj deve conter exatamente 14 dígitos.");
        if (!new EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException("BootstrapAdmin:Email possui formato inválido.");
        if (password.Length < 10)
            throw new InvalidOperationException("BootstrapAdmin:Password deve ter pelo menos 10 caracteres.");
        if (!string.IsNullOrEmpty(planRaw)
            && !new[] { "bronze", "prata", "ouro" }.Contains(planRaw.ToLowerInvariant()))
            throw new InvalidOperationException("BootstrapAdmin:Plan deve ser Bronze, Prata ou Ouro.");

        return new BootstrapInput(
            tenantName,
            cnpj,
            administratorName,
            email,
            password,
            PlanoLimites.DeTexto(planRaw));
    }

    private string Required(string key, int maxLength, bool trim = true)
    {
        var configured = _configuration[key];
        var value = trim ? configured?.Trim() : configured;
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Configuração obrigatória ausente: {key}.");
        if (value.Length > maxLength)
            throw new InvalidOperationException($"Configuração {key} excede {maxLength} caracteres.");
        return value;
    }

    private sealed record BootstrapInput(
        string TenantName,
        string Cnpj,
        string AdministratorName,
        string Email,
        string Password,
        PlanoTenant Plan);
}

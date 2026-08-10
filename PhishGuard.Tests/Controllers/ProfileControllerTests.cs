using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using Xunit;

namespace PhishGuard.Tests.Controllers;

public sealed class ProfileControllerTests
{
    private const string SenhaAtual = "SenhaAtual@123";
    private const string ChaveToken = "chave-secreta-somente-para-testes-com-tamanho-suficiente-para-hmacsha512";

    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantId { get; set; }
        public Guid GetTenantId() => TenantId;
        public Guid GetCurrentTenantId() => TenantId;
    }

    private sealed record Fixture(
        LoginController Controller,
        AppDbContext Context,
        FakeTenantProvider TenantProvider,
        Administrador Administrator,
        AuthSession CurrentSession,
        AuthSession OtherSession);

    private static async Task<Fixture> CreateFixtureAsync()
    {
        var tenantProvider = new FakeTenantProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options, tenantProvider);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            NomeEmpresa = "Empresa Perfil",
            Cnpj = "12345678901234",
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
        };
        tenantProvider.TenantId = tenant.Id;
        var administrator = new Administrador
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Nome = "Admin Original",
            Email = "admin@perfil.test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(SenhaAtual),
        };
        var currentSession = Session(administrator, "A");
        var otherSession = Session(administrator, "B");

        context.AddRange(tenant, administrator, currentSession, otherSession);
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:Token"] = ChaveToken,
                ["AppSettings:JwtIssuer"] = "PhishGuard.Backend.Tests",
                ["AppSettings:JwtAudience"] = "PhishGuard.Frontend.Tests",
                ["AppSettings:AccessTokenMinutes"] = "60",
            })
            .Build();
        var controller = new LoginController(context, configuration, TimeProvider.System);
        SetIdentity(controller, administrator.Id, tenant.Id, currentSession.Id);

        return new Fixture(controller, context, tenantProvider, administrator, currentSession, otherSession);
    }

    private static AuthSession Session(Administrador administrator, string suffix) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = administrator.TenantId,
        AdministratorId = administrator.Id,
        RefreshTokenHash = new string(suffix[0], 64),
        CreatedAtUtc = DateTime.UtcNow,
        ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
    };

    private static void SetIdentity(LoginController controller, Guid administratorId, Guid tenantId, Guid sessionId)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, administratorId.ToString()),
                    new Claim("tenant_id", tenantId.ToString()),
                    new Claim("sid", sessionId.ToString()),
                }, "Test")),
            },
        };
    }

    [Fact]
    public async Task GetProfile_ReturnsOnlyProfileDtoForAuthenticatedTenant()
    {
        var fixture = await CreateFixtureAsync();

        var result = await fixture.Controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<ProfileResponseDto>(ok.Value);
        Assert.Equal(fixture.Administrator.Nome, profile.Nome);
        Assert.Equal(fixture.Administrator.Email, profile.Email);
    }

    [Fact]
    public async Task GetProfile_WithMismatchedTenantClaim_DoesNotExposeAdministrator()
    {
        var fixture = await CreateFixtureAsync();
        var foreignTenantId = Guid.NewGuid();
        fixture.TenantProvider.TenantId = foreignTenantId;
        SetIdentity(fixture.Controller, fixture.Administrator.Id, foreignTenantId, fixture.CurrentSession.Id);

        var result = await fixture.Controller.GetProfile();

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateProfile_WithMismatchedTenantClaim_DoesNotModifyAdministrator()
    {
        var fixture = await CreateFixtureAsync();
        var originalName = fixture.Administrator.Nome;
        var foreignTenantId = Guid.NewGuid();
        fixture.TenantProvider.TenantId = foreignTenantId;
        SetIdentity(fixture.Controller, fixture.Administrator.Id, foreignTenantId, fixture.CurrentSession.Id);

        var result = await fixture.Controller.UpdateProfile(new UpdateProfileDto { Nome = "Nome Forjado" });

        Assert.IsType<NotFoundResult>(result.Result);
        var persisted = await fixture.Context.Administradores
            .IgnoreQueryFilters()
            .SingleAsync(admin => admin.Id == fixture.Administrator.Id);
        Assert.Equal(originalName, persisted.Nome);
    }

    [Fact]
    public async Task UpdateProfile_NameOnly_UpdatesDatabaseAndReturnsTokenWithNewName()
    {
        var fixture = await CreateFixtureAsync();

        var result = await fixture.Controller.UpdateProfile(new UpdateProfileDto { Nome = "  Novo Nome  " });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ProfileUpdateResponseDto>(ok.Value);
        Assert.Equal("Novo Nome", response.Nome);
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Name && claim.Value == "Novo Nome");

        var persisted = await fixture.Context.Administradores.SingleAsync();
        Assert.Equal("Novo Nome", persisted.Nome);
        Assert.Null(fixture.OtherSession.RevokedAtUtc);
    }

    [Fact]
    public async Task UpdateProfile_WithWrongCurrentPassword_DoesNotChangePasswordOrRevokeSessions()
    {
        var fixture = await CreateFixtureAsync();
        var oldHash = fixture.Administrator.PasswordHash;

        var result = await fixture.Controller.UpdateProfile(new UpdateProfileDto
        {
            SenhaAtual = "senha-incorreta",
            NovaSenha = "NovaSenha@456",
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal(oldHash, fixture.Administrator.PasswordHash);
        Assert.Null(fixture.CurrentSession.RevokedAtUtc);
        Assert.Null(fixture.OtherSession.RevokedAtUtc);
    }

    [Fact]
    public async Task UpdateProfile_WithValidPassword_ChangesHashAndRevokesOnlyOtherSessions()
    {
        var fixture = await CreateFixtureAsync();
        const string newPassword = "NovaSenha@456";

        var result = await fixture.Controller.UpdateProfile(new UpdateProfileDto
        {
            SenhaAtual = SenhaAtual,
            NovaSenha = newPassword,
        });

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, fixture.Administrator.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify(SenhaAtual, fixture.Administrator.PasswordHash));
        Assert.Null(fixture.CurrentSession.RevokedAtUtc);
        Assert.NotNull(fixture.OtherSession.RevokedAtUtc);
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using PhishGuard.Backend.Data;  
using PhishGuard.Backend.Models;
using BCrypt.Net;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Security;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.BackgroundServices;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


// Segredo do JWT resolvido pela pilha de configuração (env var AppSettings__Token,
// user-secrets em dev, etc.) — NUNCA versionado no appsettings.json.
var jwtKey = builder.Configuration.GetSection("AppSettings:Token").Value
             ?? throw new InvalidOperationException(
                 "Segredo do JWT ausente. Defina 'AppSettings__Token' (variável de ambiente/.env) " +
                 "ou 'AppSettings:Token' via user-secrets.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = false, 
        ValidateAudience = false 
    };
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Forwarded Headers: atrás do proxy/rede Docker, o IP visto pelo Kestrel é o do proxy.
// Sem isto, o rate-limit por IP (abaixo) colapsa TODOS os clientes num único IP (o do
// proxy) — um usuário legítimo seria barrado pelo tráfego de outro. Reescreve
// RemoteIpAddress a partir do X-Forwarded-For.
// SEGURANÇA: X-Forwarded-* é forjável. Confie nele APENAS quando a app só for
// alcançável através do proxy confiável (rede interna do Compose). KnownNetworks/
// KnownProxies são limpos porque o IP do proxy no Docker é dinâmico; a fronteira de
// confiança passa a ser a topologia de rede (não exponha o backend direto à internet).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Rate-limiting anti-brute-force do login: janela fixa de 5 tentativas/minuto,
// particionada pelo IP de origem. Excedeu → 429 Too Many Requests (sem enfileirar).
// Aplicado só ao endpoint /api/auth/login via [EnableRateLimiting("login")].
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
    {
        // Chave = IP de origem. Atrás de proxy/Docker, exige ForwardedHeaders para
        // não colapsar todos os clientes no IP do proxy (ver nota abaixo).
        var chaveIp = httpContext.Connection.RemoteIpAddress?.ToString()
                      ?? IPAddress.None.ToString();

        return RateLimitPartition.GetFixedWindowLimiter(chaveIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhishGuard API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT assim: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Swashbuckle 10 / Microsoft.OpenApi 2.0: AddSecurityRequirement agora recebe uma
    // Func<OpenApiDocument, OpenApiSecurityRequirement> (a referência ao scheme precisa do
    // documento hospedeiro). A referência em si deixou de ser um OpenApiSecurityScheme com
    // .Reference/OpenApiReference e passou a ser o tipo dedicado OpenApiSecuritySchemeReference.
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        // Origens liberadas vêm da configuração (AppSettings:AllowedCorsOrigins) para
        // permitir o domínio PÚBLICO do túnel (ngrok) sem recompilar — a landing servida
        // pelo domínio público precisa que o POST de submissão para o backend passe no
        // preflight de CORS. Fallback: apenas o dev local (http://localhost:5173).
        var origins = builder.Configuration
            .GetSection("AppSettings:AllowedCorsOrigins")
            .Get<string[]>();

        if (origins == null || origins.Length == 0)
            origins = new[] { "http://localhost:5173" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Data Protection: base da criptografia EM REPOUSO da senha SMTP (ISmtpCredentialProtector).
// SetApplicationName fixo + chaves persistidas em disco garantem que o valor cifrado
// continue decifrável após reinícios do processo.
// ATENÇÃO (produção/Docker): "DataProtection-Keys" precisa morar em um VOLUME PERSISTENTE
// (ou ser trocado por PersistKeysToDbContext); num container efêmero as chaves se perdem
// no rebuild e as senhas salvas ficam indecifráveis.
builder.Services.AddDataProtection()
    .SetApplicationName("PhishGuard")
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

builder.Services.AddSingleton<ISmtpCredentialProtector, SmtpCredentialProtector>();

// Sanitização anti-XSS do HTML de templates/landings (allow-list). Stateless e com
// configuração imutável de allow-list → Singleton (evita reconstruir o HtmlSanitizer
// a cada requisição).
builder.Services.AddSingleton<IHtmlSanitizationService, HtmlSanitizationService>();

// Fábrica de clientes SMTP (abstração sobre o MailKit SmtpClient). Singleton: não guarda
// estado — cada Create() devolve um cliente novo de vida curta. Permite injetar um duplo
// nos testes de resiliência do disparo.
builder.Services.AddSingleton<ISmtpClientFactory, MailKitSmtpClientFactory>();

// Serviço de disparo (reutilizado pelo botão manual e pelo worker de agendamento)
// e worker em segundo plano que dispara campanhas quando a DataInicio é atingida.
builder.Services.AddScoped<ICampaignDispatchService, CampaignDispatchService>();
builder.Services.AddHostedService<CampaignSchedulerWorker>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// PRIMEIRO middleware: reescreve o IP de origem a partir do X-Forwarded-For ANTES de
// qualquer coisa que dependa dele (rate-limiter, logs). Ordem é crítica.
app.UseForwardedHeaders();

app.UseCors("AllowReactApp");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- CÓDIGO DE AUTOMIGRAÇÃO PARA O DOCKER ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("--> Aplicando migrações pendentes no banco de dados do Docker...");
            context.Database.Migrate();
            Console.WriteLine("--> Migrações aplicadas com sucesso!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Erro ao aplicar migrações na inicialização: {ex.Message}");
    }
}
// --------------------------------------------

app.Run();
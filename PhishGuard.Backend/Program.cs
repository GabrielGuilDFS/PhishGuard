using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
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

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
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


app.UseCors("AllowReactApp");

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
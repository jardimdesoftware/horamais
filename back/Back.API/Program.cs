using Back.API.Configurations;
using Back.API.Middleware;
using Back.Application;
using Back.Infrastructure;
using Back.Infrastructure.Persistence.Context;
using Back.Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Threading;

// Carrega vari�veis do .env
DotNetEnv.Env.Load(Path.Join(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// Add appsettings.json + .env como IConfiguration
builder.Configuration.AddEnvironmentVariables();

// Logging (Serilog): registra ABSOLUTAMENTE TUDO — nível Verbose, todas as
// requisições HTTP e todas as queries do EF Core — no console e em arquivo
// diário rotacionado em /app/logs (montado no host via docker-compose.dev.yml).
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine(AppContext.BaseDirectory, "logs", "horas-discentes-.log"),
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 50_000_000,
            retainedFileCountLimit: 31,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1),
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}    {Message:lj}{NewLine}{Exception}");
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();
var corsAllowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGIN"];
builder.Services.AddCorsConfig(corsAllowedOrigins, builder.Environment.IsDevelopment());

// Banco de Dados
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
if (string.IsNullOrWhiteSpace(connectionString))
    throw new Exception("Connection string n�o definida. Verifique o .env");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    // Em desenvolvimento registra os valores dos parâmetros e erros detalhados
    // das queries, para que o log capture absolutamente tudo do acesso a dados.
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Identity
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 16)
    throw new Exception("JWT Key n�o configurada corretamente.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Worker que dispara os lembretes de pendência nas datas agendadas
builder.Services.AddHostedService<Back.API.Workers.LembreteEmailWorker>();

var app = builder.Build();

// Executa migra��o + seed admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Verifica se precisa aplicar migration
    if ((await context.Database.GetPendingMigrationsAsync()).Any())
    {
        Console.WriteLine(" Aplicando migrations...");
        context.Database.Migrate();
    }

    // Cria roles se n�o existirem
    var roles = new[] { "ALUNO", "COORDENADOR", "ADMIN" };
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Executa seed de admin
    Console.WriteLine(" Rodando seed de admin...");
    await AdminSeeder.SeedAsync(context, userManager);

    // Semeia os campi do IFPE antes do coordenador (que se vincula a Belo Jardim)
    Console.WriteLine(" Rodando seed de campi...");
    await CampusSeeder.SeedAsync(context);

    // Executa seed de coordenador
    Console.WriteLine(" Rodando seed de coordenador...");
    await CoordenadorSeeder.SeedAsync(context, userManager);

    // Atividades de referência do IFPE (complementares/extensão) em todos os ambientes
    Console.WriteLine(" Rodando seed de atividades...");
    await AtividadeSeeder.SeedAsync(context);

    // Seed de dados de desenvolvimento
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine(" Rodando seed de dados de dev...");
        await DevDataSeeder.SeedAsync(context, userManager);
    }
}

// Registra todas as requisições HTTP (método, rota, status, duração)
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(CorsConfig.PolicyName);
app.UseSwagger();
app.UseSwaggerUI();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Rodar manualmente s� o seed com --seed
if (args.Contains("--seed"))
{
    await SeedDatabaseAsync(app);
    return;
}

app.Run();

// Seed manual
async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var roles = new[] { "ALUNO", "COORDENADOR", "ADMIN" };
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    await AdminSeeder.SeedAsync(context, userManager);
    Console.WriteLine(" Seed executado com sucesso.");
}

// Expõe a classe Program (gerada pelas top-level statements) para os
// testes de integração que usam WebApplicationFactory<Program>.
public partial class Program { }

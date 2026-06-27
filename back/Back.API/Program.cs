using Back.API.Configurations;
using Back.API.Middleware;
using Back.Application;
using Back.Infrastructure;
using Back.Infrastructure.Persistence.Context;
using Back.Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var envPath = FindFileUpwards(Directory.GetCurrentDirectory(), ".env");
if (envPath is not null)
    DotNetEnv.Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

var corsAllowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGIN"];
builder.Services.AddCorsConfig(corsAllowedOrigins, builder.Environment.IsDevelopment());
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var connectionString = FirstNonEmpty(
    builder.Configuration["ConnectionStrings:DefaultConnection"],
    builder.Configuration["DATABASE_URL"]);

if (string.IsNullOrWhiteSpace(connectionString))
    throw new Exception("Connection string nao definida. Verifique ConnectionStrings__DefaultConnection ou DATABASE_URL.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 16)
    throw new Exception("JWT Key nao configurada corretamente.");

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

builder.Services.AddSignalR();
builder.Services.AddScoped<Back.Application.Interfaces.Services.ITurmaRealtimeNotifier,
    Back.API.Hubs.TurmaRealtimeNotifier>();
builder.Services.AddScoped<Back.Application.Interfaces.Services.ICertificadoRealtimeNotifier,
    Back.API.Hubs.CertificadoRealtimeNotifier>();

builder.Services.AddHostedService<Back.API.Workers.LembreteEmailWorker>();

var app = builder.Build();

if (args.Contains("--seed"))
{
    await SeedDatabaseAsync(app, includeDevelopmentData: false);
    return;
}

await SeedDatabaseAsync(app, includeDevelopmentData: app.Environment.IsDevelopment());

app.UseForwardedHeaders();
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
app.MapHub<Back.API.Hubs.TurmaHub>("/hubs/turma");
app.MapHub<Back.API.Hubs.CertificadoHub>("/hubs/certificado");

app.Run();

static string? FindFileUpwards(string startDirectory, string fileName)
{
    var directory = new DirectoryInfo(startDirectory);

    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, fileName);
        if (File.Exists(candidate))
            return candidate;

        directory = directory.Parent;
    }

    return null;
}

static string? FirstNonEmpty(params string?[] values)
{
    return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}

async Task SeedDatabaseAsync(WebApplication app, bool includeDevelopmentData)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if ((await context.Database.GetPendingMigrationsAsync()).Any())
    {
        Console.WriteLine(" Aplicando migrations...");
        await context.Database.MigrateAsync();
    }

    var roles = new[] { "ALUNO", "COORDENADOR", "ADMIN" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    Console.WriteLine(" Rodando seed de admin...");
    await AdminSeeder.SeedAsync(context, userManager);

    Console.WriteLine(" Rodando seed de campi...");
    await CampusSeeder.SeedAsync(context);

    Console.WriteLine(" Rodando seed de coordenador...");
    await CoordenadorSeeder.SeedAsync(context, userManager);

    Console.WriteLine(" Rodando seed de atividades...");
    await AtividadeSeeder.SeedAsync(context);

    if (includeDevelopmentData)
    {
        Console.WriteLine(" Rodando seed de dados de dev...");
        await DevDataSeeder.SeedAsync(context, userManager);
    }

    Console.WriteLine(" Seeds executados com sucesso.");
}

public partial class Program { }

using Back.API.Configurations;
using Back.API.Middleware;
using Back.API.Services;
using Back.Application;
using Back.Infrastructure;
using Back.Infrastructure.Persistence.Context;
using Back.Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

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
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker", Serilog.Events.LogEventLevel.Warning)
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
builder.Services.AddScoped<ResourceAuthorizationService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 300,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

var corsAllowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGIN"];
builder.Services.AddCorsConfig(corsAllowedOrigins, builder.Environment.IsDevelopment());
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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
        options.EnableDetailedErrors();
    }
});

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
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
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var principal = context.Principal;
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var stamp = principal?.FindFirst("security_stamp")?.Value;
            if (string.IsNullOrWhiteSpace(userId) || stamp is null)
            {
                context.Fail("Sessão inválida.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var identity = await db.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.SecurityStamp })
                .SingleOrDefaultAsync();
            if (identity is null || identity.SecurityStamp != stamp)
            {
                context.Fail("Sessão revogada.");
                return;
            }

            var role = principal!.FindFirst(ClaimTypes.Role)?.Value;
            if (role is null || !await db.UserRoles
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .AnyAsync(x => x.UserId == userId && x.Name == role))
            {
                context.Fail("Perfil revogado.");
                return;
            }

            if (role == "ALUNO" && !await db.Alunos.AnyAsync(a => a.IdentityUserId == userId && a.IsAtivo))
                context.Fail("Aluno inativo.");
            else if (role == "COORDENADOR" && !await db.Coordenadores.AnyAsync(c => c.IdentityUserId == userId))
                context.Fail("Coordenador removido.");
            else if (role == "ADMIN" && !await db.Admins.AnyAsync(a => a.IdentityUserId == userId))
                context.Fail("Administrador removido.");
        }
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
app.UseRouting();
app.UseCors(CorsConfig.PolicyName);
app.UseRateLimiter();
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

    await LegacySeedCredentialRemediator.RunAsync(userManager);

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

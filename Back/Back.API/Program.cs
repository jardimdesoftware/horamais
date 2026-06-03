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
using System.Text;
using System.Threading;

// Carrega vari�veis do .env
DotNetEnv.Env.Load(Path.Join(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// Add appsettings.json + .env como IConfiguration
builder.Configuration.AddEnvironmentVariables();

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
    options.UseNpgsql(connectionString));

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

    // Executa seed de coordenador
    Console.WriteLine(" Rodando seed de coordenador...");
    await CoordenadorSeeder.SeedAsync(context, userManager);

    // Seed de dados de desenvolvimento
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine(" Rodando seed de dados de dev...");
        await DevDataSeeder.SeedAsync(context, userManager);
    }
}

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

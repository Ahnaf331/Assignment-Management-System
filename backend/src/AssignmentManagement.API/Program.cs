using System.Text;
using System.Text.Json.Serialization;
using AssignmentManagement.API.Filters;
using AssignmentManagement.API.Middleware;
using AssignmentManagement.API.Services;
using AssignmentManagement.Application;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Infrastructure;
using AssignmentManagement.Infrastructure.Persistence;
using AssignmentManagement.Infrastructure.Persistence.Seed;
using AssignmentManagement.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Logging (Serilog) ----------------
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console()
          .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

// ---------------- Application + Infrastructure ----------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Current-user abstraction resolved from the HTTP context.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ---------------- Controllers + validation ----------------
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings for a friendlier API contract.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ---------------- Authentication (JWT) ----------------
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JWT settings are not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// ---------------- CORS ----------------
const string CorsPolicy = "FrontendCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// ---------------- Swagger / OpenAPI ----------------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Assignment & Submission Management API",
        Version = "v1",
        Description = "Role-based API for managing assignments and submissions (Admin / Teacher / Student)."
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

// ---------------- Database migration + seeding ----------------
await MigrateAndSeedAsync(app);

// ---------------- HTTP pipeline ----------------
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment Management API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task MigrateAndSeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var configuration = services.GetRequiredService<IConfiguration>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var hasher = services.GetRequiredService<IPasswordHasher>();
        await DbSeeder.SeedAsync(context, hasher, logger);
    }
    catch (Exception ex)
    {
        // A first-run database connection failure is almost always missing local
        // configuration rather than a bug, so print actionable setup guidance
        // instead of leaving the developer with a bare stack trace.
        PrintDatabaseSetupHelp(configuration, ex);
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

static void PrintDatabaseSetupHelp(IConfiguration configuration, Exception ex)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    var placeholderInUse = connectionString.Contains("YOUR_POSTGRES_PASSWORD", StringComparison.OrdinalIgnoreCase);
    var isAuthFailure = ex.ToString().Contains("28P01") || ex.ToString().Contains("password authentication failed");

    if (!placeholderInUse && !isAuthFailure) return;

    var previous = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;

    Console.Error.WriteLine();
    Console.Error.WriteLine("========================================================================");
    Console.Error.WriteLine(" DATABASE SETUP REQUIRED");
    Console.Error.WriteLine("========================================================================");
    Console.Error.WriteLine();
    Console.Error.WriteLine(placeholderInUse
        ? " The connection string still contains the placeholder 'YOUR_POSTGRES_PASSWORD'."
        : " Could not authenticate against PostgreSQL with the configured password.");
    Console.Error.WriteLine();
    Console.Error.WriteLine(" Set your local PostgreSQL password using EITHER option:");
    Console.Error.WriteLine();
    Console.Error.WriteLine(" 1) Local settings file (recommended)");
    Console.Error.WriteLine("      cd backend/src/AssignmentManagement.API");
    Console.Error.WriteLine("      copy appsettings.Development.json.example appsettings.Development.json");
    Console.Error.WriteLine("    then edit it and replace YOUR_POSTGRES_PASSWORD with your password.");
    Console.Error.WriteLine("    (This file is gitignored, so your password is never committed.)");
    Console.Error.WriteLine();
    Console.Error.WriteLine(" 2) Environment variable");
    Console.Error.WriteLine("      $env:ConnectionStrings__DefaultConnection=\"Host=localhost;Port=5432;\" +");
    Console.Error.WriteLine("        \"Database=assignment_management;Username=postgres;Password=YOUR_PASSWORD\"");
    Console.Error.WriteLine();
    Console.Error.WriteLine(" Also make sure the PostgreSQL service is running on localhost:5432.");
    Console.Error.WriteLine(" Full details: see 'Configure your database password' in README.md");
    Console.Error.WriteLine("========================================================================");
    Console.Error.WriteLine();

    Console.ForegroundColor = previous;
}

// Exposed so the test project can reference the entry point if needed.
public partial class Program { }

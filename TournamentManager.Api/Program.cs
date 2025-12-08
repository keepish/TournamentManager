using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using System.Text;
using TournamentManager.Core.Models;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = null;

var solutionRootEnvPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"));
if (File.Exists(solutionRootEnvPath))
{
    Env.Load(solutionRootEnvPath);
}

static string? GetVar(string key) => Env.GetString(key) ?? Environment.GetEnvironmentVariable(key);

// Prefer connection string provided via configuration (e.g., docker-compose)
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? builder.Configuration["ConnectionStrings__DefaultConnection"];

if (!string.IsNullOrWhiteSpace(defaultConnection))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(defaultConnection));
}
else
{
    // Prefer container host when running inside Docker; fallback to localhost only for true local dev
    var runningInContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
    var dbName = GetVar("DB_NAME");
    var dbUser = GetVar("DB_USER");
    var dbPassword = GetVar("DB_PASSWORD");

    if (!string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPassword))
    {       
        connectionString = $"Server=db;Port=3306;Database={dbName};User={dbUser};Password={dbPassword};AllowPublicKeyRetrieval=True;SslMode=None";
        builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(connectionString));
    }
    else
    {
        throw new InvalidOperationException("No valid MySQL connection string found. Set ConnectionStrings__DefaultConnection or DB_NAME/DB_USER/DB_PASSWORD.");
    }
}

// Поддержка переменных окружения с альтернативными именами (на случай неверного маппинга)
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? builder.Configuration["JWT_Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["JWT_Audience"];
var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["JWT_Key"];


builder.Services.AddResponseCompression();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey ?? throw new InvalidOperationException("Jwt:Key is not set")))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable response compression if registered
app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        logger.LogError(
            exceptionHandler?.Error,
            "Произошло необработанное исключение по пути: {Path}.",
            exceptionHandler?.Path
        );

        await context.Response.WriteAsJsonAsync("Что-то пошло не так, повторите попытку или попробуйте позже");
    });
});


app.Run();

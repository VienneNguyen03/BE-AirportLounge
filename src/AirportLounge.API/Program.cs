using AirportLounge.API.Filters;
using AirportLounge.API.Middleware;
using AirportLounge.Application;
using AirportLounge.Infrastructure;
using AirportLounge.Infrastructure.Hubs;
using AirportLounge.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Application & Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers – serialize enums as strings
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// RESTful lowercase routes: /api/auth, /api/employees, /api/zones …
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Airport Lounge Staff Management API",
        Version = "v1",
        Description = "Internal management system for airport lounge staff"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
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
            Array.Empty<string>()
        }
    });

    c.SchemaFilter<EnumSchemaFilter>();
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Apply EF migrations and seed admin on startup (Docker/local)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    
    logger.LogInformation("[DataSeeder] Starting seeding processes...");
    await AirportLounge.Persistence.DataSeeder.SeedAdminAsync(db);
    await AirportLounge.Persistence.DataSeeder.SeedDemoStaffAsync(db);
    await AirportLounge.Persistence.DataSeeder.SeedMasterDataAsync(db);
    await AirportLounge.Persistence.DataSeeder.SeedTaskCategoriesAsync(db);
    await AirportLounge.Persistence.DataSeeder.SeedIdCardTemplatesAsync(db);
    logger.LogInformation("[DataSeeder] Seeding processes completed.");

    // Clear caches to ensure UI shows new seed data
    var cache = scope.ServiceProvider.GetService<AirportLounge.Application.Common.Interfaces.ICacheService>();
    if (cache != null)
    {
        logger.LogInformation("[Cache] Clearing master data caches...");
        await cache.RemoveAsync(AirportLounge.Application.Common.CacheKeys.LeaveTypesList);
        await cache.RemoveAsync(AirportLounge.Application.Common.CacheKeys.TaskCategoriesList);
        await cache.RemoveAsync(AirportLounge.Application.Common.CacheKeys.DepartmentsList);
        await cache.RemoveAsync(AirportLounge.Application.Common.CacheKeys.PositionsList);
        await cache.RemoveAsync(AirportLounge.Application.Common.CacheKeys.SkillsList);
        logger.LogInformation("[Cache] Caches cleared.");
    }
}

// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Airport Lounge API v1"));
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.UseSerilogRequestLogging();

app.Run();

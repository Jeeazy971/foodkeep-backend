using FoodKeep.API.Extensions;
using FoodKeep.Application;
using FoodKeep.Infrastructure;
using FoodKeep.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// ========================================
// Configuration Serilog (Early initialization)
// ========================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting FoodKeep API");
    Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

    var builder = WebApplication.CreateBuilder(args);

    // ========================================
    // Serilog Configuration
    // ========================================
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Conditional(
            condition => context.HostingEnvironment.IsProduction(),
            writeTo => writeTo.File(
                path: "logs/foodkeep-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")));

    // ========================================
    // Add Services to Container
    // ========================================

    // Core Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database")
        .AddRedis(
            builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379",
            name: "redis",
            tags: new[] { "cache" });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy
                .WithOrigins(
                    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                    new[] { "http://localhost:3000", "http://localhost:4200" })
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });

        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    // Add Clean Architecture Layers
    builder.Services.AddApplication();                    // Application Layer
    builder.Services.AddInfrastructure(builder.Configuration); // Infrastructure Layer

    // API Services (from extension method)
    builder.Services.AddApiServices(builder.Configuration);

    // Swagger Documentation
    builder.Services.AddSwaggerDocumentation();

    // Authentication & Authorization
    builder.Services.AddAuthentication(builder.Configuration);
    builder.Services.AddAuthorization();

    // ========================================
    // Build Application
    // ========================================
    var app = builder.Build();

    // ========================================
    // Configure HTTP Request Pipeline
    // ========================================

    // Global Exception Handler (first in pipeline)
    app.UseExceptionHandler("/error");

    // Development specific
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();

        // Migrate database automatically in development
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase") == false)
        {
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrated successfully");
        }
    }

    // Security Headers
    app.UseSecurityHeaders();

    // Request Logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? LogEventLevel.Error
                : LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value!);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown");
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
        };
    });

    // HTTPS Redirection (skip in Docker)
    if (!IsRunningInContainer())
    {
        app.UseHttpsRedirection();
    }

    // CORS
    app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowSpecificOrigins");

    // Swagger (all environments for now, can be restricted later)
    app.UseSwaggerDocumentation();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Rate Limiting
    app.UseRateLimiter();

    // Endpoints
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    // Redirect root to Swagger
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

    // Fallback
    app.MapFallback(() => Results.NotFound(new
    {
        error = "Endpoint not found",
        documentation = "/swagger"
    }));

    // ========================================
    // Start Application
    // ========================================
    Log.Information("FoodKeep API started successfully");
    Log.Information("Swagger available at: {SwaggerUrl}/swagger", app.Urls.FirstOrDefault());

    await app.RunAsync();
}
catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down FoodKeep API");
    await Log.CloseAndFlushAsync();
}

// ========================================
// Helper Methods
// ========================================
static bool IsRunningInContainer() =>
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
    Environment.GetEnvironmentVariable("DOCKER_CONTAINER") == "true";

// ========================================
// Partial class for integration tests
// ========================================
public partial class Program
{
    // Expose for integration tests
}
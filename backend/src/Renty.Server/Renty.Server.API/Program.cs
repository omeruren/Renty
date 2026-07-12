using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Renty.Server.API.Endpoints;
using Renty.Server.API.Middleware;
using Renty.Server.Application;
using Renty.Server.Infrastructure;
using Renty.Server.Infrastructure.Configuration;
using Renty.Server.Persistence;
using Renty.Server.Persistence.Seed;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Renty API host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // Add services to the container.
    builder.Services.AddPersistenceServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = "sub",
                RoleClaimType = "roles"
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("CanManageFleet", policy => policy.RequireRole("Admin", "Manager"))
        .AddPolicy("CanManageReservations", policy => policy.RequireRole("Admin", "Manager"))
        .AddPolicy("CanManagePricing", policy => policy.RequireRole("Admin", "Manager"))
        .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
        .AddPolicy("CanViewReports", policy => policy.RequireRole("Admin", "Manager"));

    // No AllowedOrigins configured means no cross-origin requests are permitted — fail closed
    // rather than falling back to a wildcard (docs/05-Security-Architecture.md §6). Bound lazily
    // via IConfiguration injection (same pattern as JwtSettings above) rather than an eager
    // builder.Configuration.Get<string[]>() read — WebApplicationFactory-based integration tests
    // inject their Cors:AllowedOrigins override during Build(), so reading it any earlier sees an
    // unmerged, empty value.
    builder.Services.AddCors();
    builder.Services.AddOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>()
        .Configure<IConfiguration>((options, configuration) =>
        {
            var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            options.AddDefaultPolicy(policy => policy
                .WithOrigins(corsOrigins)
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                .WithHeaders("Authorization", "Content-Type")
                .AllowCredentials());
        });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("auth-refresh", limiterOptions =>
        {
            limiterOptions.PermitLimit = 10;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 0;
        });

        // Read/write/admin caps from docs/05-Security-Architecture.md §6, applied globally by
        // client IP so /auth/* (which already has its own stricter named limiters above) and
        // /health/* aren't double-restricted, and so every other endpoint is covered without
        // annotating each of the ~60 individual routes across the 10 feature slices.
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (path.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
                return RateLimitPartition.GetNoLimiter("exempt");

            var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var isAdminRoute = path.StartsWith("/api/v1/users", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/api/v1/audit-logs", StringComparison.OrdinalIgnoreCase);

            var category = isAdminRoute ? "admin" : HttpMethods.IsGet(context.Request.Method) ? "read" : "write";

            var permitLimit = category switch
            {
                "admin" => 50,
                "write" => 30,
                _ => 100
            };

            return RateLimitPartition.GetFixedWindowLimiter($"{category}:{clientId}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
        });
    });

    builder.Services.AddHealthChecks()
        .AddSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "database",
            tags: ["ready"])
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Checked post-Build (rather than eagerly against builder.Configuration) so this reads the
    // fully-merged configuration — WebApplicationFactory-based integration tests inject their
    // Jwt:SecretKey override during Build(), and reading it any earlier sees an unmerged, null
    // value. Fail secure: HS256 with a short key is silently weak rather than broken, so refuse
    // to boot rather than sign tokens with an under-strength secret
    // (docs/05-Security-Architecture.md requires a minimum 256-bit / 32-byte key).
    var resolvedJwtSecretKey = app.Services.GetRequiredService<IOptions<JwtSettings>>().Value.SecretKey;
    if (Encoding.UTF8.GetByteCount(resolvedJwtSecretKey) < 32)
        throw new InvalidOperationException("Jwt:SecretKey must be at least 256 bits (32 bytes) long.");

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await DevelopmentDataSeeder.SeedAsync(dbContext);
    }

    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseMiddleware<TraceIdLoggingMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseExceptionHandler();
    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthEndpoints();
    app.MapBrandEndpoints();
    app.MapModelEndpoints();
    app.MapCarEndpoints();
    app.MapReservationEndpoints();
    app.MapLocationEndpoints();
    app.MapPricingRuleEndpoints();
    app.MapUserEndpoints();
    app.MapProfileEndpoints();
    app.MapAuditLogEndpoints();
    app.MapReportEndpoints();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException is expected here: WebApplicationFactory<Program> (integration tests)
    // and `dotnet ef` design-time tooling both build this host just far enough to discover its
    // configuration, then intentionally abort it — that's not a real startup failure.
    Log.Fatal(ex, "Renty API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API host against an isolated in-memory database instead of SQL Server, with
/// a fake authentication scheme so tests can assert against role-based policies without minting
/// real JWTs. Program.cs requires Jwt:SecretKey/ConnectionStrings:DefaultConnection at startup
/// and seeds dev data only in the Development environment, so both are addressed here.
/// </summary>
public sealed class RentyApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"RentyTestDb_{Guid.NewGuid()}";

    // EF Core registers each provider's internal services directly into the app's service
    // collection. Program.cs already registered the SqlServer provider's services, so simply
    // swapping the DbContextOptions descriptor leaves both providers' services side by side and
    // EF throws "Only a single database provider can be registered". Giving the InMemory
    // provider its own internal service provider (the documented workaround) avoids the clash.
    private static readonly IServiceProvider InMemoryProvider =
        new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-for-integration-tests-only-do-not-use-in-production",
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(local);Database=RentyIntegrationTests;Trusted_Connection=True;TrustServerCertificate=True",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(InMemoryProvider);
            });

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}

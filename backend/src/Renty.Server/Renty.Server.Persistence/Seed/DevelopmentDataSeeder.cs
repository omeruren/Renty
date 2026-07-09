using Microsoft.EntityFrameworkCore;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Persistence.Seed;

/// <summary>
/// Seeds sample data for local development only, per docs/04-Database-Design.md "Development Data".
/// Runs at startup (not via migration HasData) so it never touches Staging/Production.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Locations.AnyAsync(cancellationToken))
            return;

        context.Locations.AddRange(
            new Location
            {
                Name = "Renty Downtown",
                Address = "123 Main Street",
                City = "Istanbul",
                District = "Kadikoy",
                PhoneNumber = "+90 216 555 0101",
                Email = "downtown@renty.dev",
                IsActive = true
            },
            new Location
            {
                Name = "Renty Airport",
                Address = "Istanbul Airport, Terminal 1",
                City = "Istanbul",
                District = "Arnavutkoy",
                PhoneNumber = "+90 212 555 0202",
                Email = "airport@renty.dev",
                IsActive = true
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}

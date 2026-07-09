using Microsoft.EntityFrameworkCore;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Model> Models { get; }
    DbSet<Car> Cars { get; }
    DbSet<Location> Locations { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<PricingRule> PricingRules { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

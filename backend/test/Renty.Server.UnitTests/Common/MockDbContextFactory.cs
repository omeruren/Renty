using MockQueryable.NSubstitute;
using NSubstitute;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;

namespace Renty.Server.UnitTests.Common;

public static class MockDbContextFactory
{
    public static IApplicationDbContext Create(
        IEnumerable<User>? users = null,
        IEnumerable<Brand>? brands = null,
        IEnumerable<Model>? models = null,
        IEnumerable<Car>? cars = null,
        IEnumerable<Location>? locations = null,
        IEnumerable<Reservation>? reservations = null,
        IEnumerable<PricingRule>? pricingRules = null,
        IEnumerable<AuditLog>? auditLogs = null)
    {
        var context = Substitute.For<IApplicationDbContext>();

        // Each mock DbSet must be built into a local variable before assigning it via Returns() —
        // BuildMockDbSet() calls Returns() internally, which would otherwise clobber NSubstitute's
        // "last call" tracking for the context.Xyz.Returns(...) call below it.
        var usersDbSet = (users ?? []).ToList().BuildMockDbSet();
        var brandsDbSet = (brands ?? []).ToList().BuildMockDbSet();
        var modelsDbSet = (models ?? []).ToList().BuildMockDbSet();
        var carsDbSet = (cars ?? []).ToList().BuildMockDbSet();
        var locationsDbSet = (locations ?? []).ToList().BuildMockDbSet();
        var reservationsDbSet = (reservations ?? []).ToList().BuildMockDbSet();
        var pricingRulesDbSet = (pricingRules ?? []).ToList().BuildMockDbSet();
        var auditLogsDbSet = (auditLogs ?? []).ToList().BuildMockDbSet();

        context.Users.Returns(usersDbSet);
        context.Brands.Returns(brandsDbSet);
        context.Models.Returns(modelsDbSet);
        context.Cars.Returns(carsDbSet);
        context.Locations.Returns(locationsDbSet);
        context.Reservations.Returns(reservationsDbSet);
        context.PricingRules.Returns(pricingRulesDbSet);
        context.AuditLogs.Returns(auditLogsDbSet);

        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return context;
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reservations.Commands.CreateReservation;
using Renty.Server.Domain.Enums;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Reservations.Commands.CreateReservation;

public sealed class CreateReservationHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService(Guid? userId)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    private static CreateReservationCommand BuildCommand(
        Guid carId, Guid pickupLocationId, Guid returnLocationId, DateTime startDate, DateTime endDate) =>
        new(carId, startDate, endDate, pickupLocationId, returnLocationId, "First rental");

    [Fact]
    public async Task Handle_ValidCommand_ReturnsReservationResponseAndBlocksCar()
    {
        var user = new UserBuilder().Build();
        var brand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(brand.Id).Build();
        var location = new LocationBuilder().Build();
        var car = new CarBuilder().WithBrand(brand).WithModel(model).WithLocationId(location.Id).WithDailyPrice(500m).WithStatus(CarStatus.Available).Build();

        var context = MockDbContextFactory.Create(
            users: [user], brands: [brand], models: [model], cars: [car], locations: [location]);
        var handler = new CreateReservationHandler(context, CreateCurrentUserService(user.Id), NullLogger<CreateReservationHandler>.Instance);

        var startDate = DateTime.UtcNow.AddDays(2);
        var endDate = startDate.AddDays(3);
        var command = BuildCommand(car.Id, location.Id, location.Id, startDate, endDate);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ReservationStatus.Pending);
        result.TotalPrice.Should().Be(1500m);
        result.CarId.Should().Be(car.Id);
        result.UserId.Should().Be(user.Id);
        car.Status.Should().Be(CarStatus.Reserved);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoSignedInUser_ThrowsUnauthorizedException()
    {
        var car = new CarBuilder().WithStatus(CarStatus.Available).Build();
        var context = MockDbContextFactory.Create(cars: [car]);
        var handler = new CreateReservationHandler(context, CreateCurrentUserService(null), NullLogger<CreateReservationHandler>.Instance);
        var command = BuildCommand(car.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(5));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_CarNotAvailable_ThrowsBusinessRuleException()
    {
        var user = new UserBuilder().Build();
        var car = new CarBuilder().WithStatus(CarStatus.Rented).Build();
        var context = MockDbContextFactory.Create(users: [user], cars: [car]);
        var handler = new CreateReservationHandler(context, CreateCurrentUserService(user.Id), NullLogger<CreateReservationHandler>.Instance);
        var command = BuildCommand(car.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(5));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*not available for reservation*");
    }

    [Fact]
    public async Task Handle_InactivePickupLocation_ThrowsBusinessRuleException()
    {
        var user = new UserBuilder().Build();
        var car = new CarBuilder().WithStatus(CarStatus.Available).Build();
        var location = new LocationBuilder().WithIsActive(false).Build();
        var context = MockDbContextFactory.Create(users: [user], cars: [car], locations: [location]);
        var handler = new CreateReservationHandler(context, CreateCurrentUserService(user.Id), NullLogger<CreateReservationHandler>.Instance);
        var command = BuildCommand(car.Id, location.Id, location.Id, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(5));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*pickup location is not active*");
    }

    [Fact]
    public async Task Handle_OverlappingBlockingReservationExists_ThrowsBusinessRuleException()
    {
        var user = new UserBuilder().Build();
        var brand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(brand.Id).Build();
        var location = new LocationBuilder().Build();
        var car = new CarBuilder().WithBrand(brand).WithModel(model).WithLocationId(location.Id).WithStatus(CarStatus.Available).Build();

        var existingStart = DateTime.UtcNow.AddDays(3);
        var existingEnd = DateTime.UtcNow.AddDays(6);
        var existingReservation = new ReservationBuilder()
            .WithCarId(car.Id)
            .WithStatus(ReservationStatus.Confirmed)
            .WithDates(existingStart, existingEnd)
            .Build();

        var context = MockDbContextFactory.Create(
            users: [user], brands: [brand], models: [model], cars: [car], locations: [location], reservations: [existingReservation]);
        var handler = new CreateReservationHandler(context, CreateCurrentUserService(user.Id), NullLogger<CreateReservationHandler>.Instance);

        // Overlaps with the existing 3-6 day window.
        var command = BuildCommand(car.Id, location.Id, location.Id, DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(7));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*not available for the requested dates*");
    }
}

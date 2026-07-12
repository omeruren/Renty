using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Locations.Commands.DeleteLocation;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Locations.Commands.DeleteLocation;

public sealed class DeleteLocationHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesLocation()
    {
        var location = new LocationBuilder().Build();
        var context = MockDbContextFactory.Create(locations: [location]);
        var handler = new DeleteLocationHandler(context, CreateCurrentUserService(), NullLogger<DeleteLocationHandler>.Instance);

        await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        context.Locations.Received(1).Remove(Arg.Is<Renty.Server.Domain.Entities.Location>(l => l.Id == location.Id));
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownLocation_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new DeleteLocationHandler(context, CreateCurrentUserService(), NullLogger<DeleteLocationHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteLocationCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_LocationHasCars_ThrowsBusinessRuleException()
    {
        var location = new LocationBuilder().Build();
        var car = new CarBuilder().WithLocationId(location.Id).Build();
        var context = MockDbContextFactory.Create(locations: [location], cars: [car]);
        var handler = new DeleteLocationHandler(context, CreateCurrentUserService(), NullLogger<DeleteLocationHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*assigned cars*");
    }

    [Fact]
    public async Task Handle_LocationReferencedByReservation_ThrowsBusinessRuleException()
    {
        var location = new LocationBuilder().Build();
        var reservation = new ReservationBuilder().WithPickupLocationId(location.Id).Build();
        var context = MockDbContextFactory.Create(locations: [location], reservations: [reservation]);
        var handler = new DeleteLocationHandler(context, CreateCurrentUserService(), NullLogger<DeleteLocationHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*reservation*");
    }
}

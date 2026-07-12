using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Locations.Commands.UpdateLocation;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Locations.Commands.UpdateLocation;

public sealed class UpdateLocationHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    private static UpdateLocationCommand BuildCommand(Guid id, bool isActive) =>
        new(id, "Downtown", "123 Main St", "Istanbul", null, null, null, null, null, isActive);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedLocationResponse()
    {
        var location = new LocationBuilder().Build();
        var context = MockDbContextFactory.Create(locations: [location]);
        var handler = new UpdateLocationHandler(context, CreateCurrentUserService(), NullLogger<UpdateLocationHandler>.Instance);

        var result = await handler.Handle(BuildCommand(location.Id, true), CancellationToken.None);

        result.Name.Should().Be("Downtown");
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownLocation_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new UpdateLocationHandler(context, CreateCurrentUserService(), NullLogger<UpdateLocationHandler>.Instance);

        var act = async () => await handler.Handle(BuildCommand(Guid.NewGuid(), true), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DeactivatingLocationWithAssignedCars_ThrowsBusinessRuleException()
    {
        var location = new LocationBuilder().WithIsActive(true).Build();
        var car = new CarBuilder().WithLocationId(location.Id).Build();
        var context = MockDbContextFactory.Create(locations: [location], cars: [car]);
        var handler = new UpdateLocationHandler(context, CreateCurrentUserService(), NullLogger<UpdateLocationHandler>.Instance);

        var act = async () => await handler.Handle(BuildCommand(location.Id, false), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_DeactivatingLocationWithNoAssignedCars_Succeeds()
    {
        var location = new LocationBuilder().WithIsActive(true).Build();
        var context = MockDbContextFactory.Create(locations: [location]);
        var handler = new UpdateLocationHandler(context, CreateCurrentUserService(), NullLogger<UpdateLocationHandler>.Instance);

        var result = await handler.Handle(BuildCommand(location.Id, false), CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }
}

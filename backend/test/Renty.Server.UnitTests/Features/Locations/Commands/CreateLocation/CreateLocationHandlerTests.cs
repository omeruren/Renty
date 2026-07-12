using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Locations.Commands.CreateLocation;
using Renty.Server.UnitTests.Common;

namespace Renty.Server.UnitTests.Features.Locations.Commands.CreateLocation;

public sealed class CreateLocationHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsActiveLocationResponse()
    {
        var context = MockDbContextFactory.Create();
        var handler = new CreateLocationHandler(context, CreateCurrentUserService(), NullLogger<CreateLocationHandler>.Instance);
        var command = new CreateLocationCommand("Downtown", "123 Main St", "Istanbul", "Kadikoy", "555-1234", "downtown@renty.dev", 40.99, 29.03);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Downtown");
        result.IsActive.Should().BeTrue();
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

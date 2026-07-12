using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Users.Commands.DeactivateUser;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_DeactivatesUser()
    {
        var user = new UserBuilder().WithIsActive(true).Build();
        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new DeactivateUserHandler(context, CreateCurrentUserService(), NullLogger<DeactivateUserHandler>.Instance);

        await handler.Handle(new DeactivateUserCommand(user.Id), CancellationToken.None);

        user.IsActive.Should().BeFalse();
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownUser_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new DeactivateUserHandler(context, CreateCurrentUserService(), NullLogger<DeactivateUserHandler>.Instance);

        var act = async () => await handler.Handle(new DeactivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

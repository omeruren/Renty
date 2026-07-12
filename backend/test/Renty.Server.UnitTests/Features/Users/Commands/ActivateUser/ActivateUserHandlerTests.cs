using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Users.Commands.ActivateUser;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Users.Commands.ActivateUser;

public sealed class ActivateUserHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ActivatesUser()
    {
        var user = new UserBuilder().WithIsActive(false).Build();
        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new ActivateUserHandler(context, CreateCurrentUserService(), NullLogger<ActivateUserHandler>.Instance);

        await handler.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        user.IsActive.Should().BeTrue();
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownUser_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new ActivateUserHandler(context, CreateCurrentUserService(), NullLogger<ActivateUserHandler>.Instance);

        var act = async () => await handler.Handle(new ActivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

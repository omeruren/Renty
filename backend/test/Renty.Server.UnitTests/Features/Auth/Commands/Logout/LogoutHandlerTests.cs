using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.Commands.Logout;
using Renty.Server.Domain.Entities;
using Renty.Server.UnitTests.Common;

namespace Renty.Server.UnitTests.Features.Auth.Commands.Logout;

public sealed class LogoutHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService(Guid? userId)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidRequest_RevokesActiveTokens()
    {
        var userId = Guid.NewGuid();
        var activeToken = new RefreshToken { UserId = userId, TokenHash = "abc", ExpiresAt = DateTime.UtcNow.AddDays(1), IsRevoked = false };
        var context = MockDbContextFactory.Create(refreshTokens: [activeToken]);
        var handler = new LogoutHandler(context, CreateCurrentUserService(userId), NullLogger<LogoutHandler>.Instance);

        await handler.Handle(new LogoutCommand(), CancellationToken.None);

        activeToken.IsRevoked.Should().BeTrue();
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoSignedInUser_ThrowsUnauthorizedException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new LogoutHandler(context, CreateCurrentUserService(null), NullLogger<LogoutHandler>.Instance);

        var act = async () => await handler.Handle(new LogoutCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}

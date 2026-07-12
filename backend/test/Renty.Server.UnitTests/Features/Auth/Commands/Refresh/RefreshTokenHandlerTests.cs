using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.Commands.Refresh;
using Renty.Server.Domain.Entities;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    private static IJwtTokenService CreateJwtTokenService(string incomingHash)
    {
        var jwtTokenService = Substitute.For<IJwtTokenService>();
        jwtTokenService.HashToken("incoming-token").Returns(incomingHash);
        jwtTokenService.HashToken(Arg.Is<string>(t => t != "incoming-token")).Returns("new-hashed-token");
        jwtTokenService.GenerateRefreshToken().Returns("new-refresh-token");
        jwtTokenService.GenerateAccessToken(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<IReadOnlyCollection<string>>())
            .Returns(("new-access-token", DateTime.UtcNow.AddMinutes(15)));
        jwtTokenService.RefreshTokenLifetime.Returns(TimeSpan.FromDays(7));
        return jwtTokenService;
    }

    [Fact]
    public async Task Handle_ValidToken_RotatesTokenAndReturnsAuthResponse()
    {
        var role = new RoleBuilder().WithName("Customer").Build();
        var user = new UserBuilder().Build();
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role });

        var token = new RefreshToken
        {
            UserId = user.Id,
            User = user,
            TokenHash = "existing-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        var context = MockDbContextFactory.Create(refreshTokens: [token]);
        var handler = new RefreshTokenHandler(
            context, CreateJwtTokenService("existing-hash"), CreateCurrentUserService(), NullLogger<RefreshTokenHandler>.Instance);

        var result = await handler.Handle(new RefreshTokenCommand("incoming-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        token.IsRevoked.Should().BeTrue();
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsUnauthorizedException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new RefreshTokenHandler(
            context, CreateJwtTokenService("some-hash"), CreateCurrentUserService(), NullLogger<RefreshTokenHandler>.Instance);

        var act = async () => await handler.Handle(new RefreshTokenCommand("incoming-token"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsUnauthorizedException()
    {
        var user = new UserBuilder().Build();
        var token = new RefreshToken
        {
            UserId = user.Id,
            User = user,
            TokenHash = "existing-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        var context = MockDbContextFactory.Create(refreshTokens: [token]);
        var handler = new RefreshTokenHandler(
            context, CreateJwtTokenService("existing-hash"), CreateCurrentUserService(), NullLogger<RefreshTokenHandler>.Instance);

        var act = async () => await handler.Handle(new RefreshTokenCommand("incoming-token"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_RevokedToken_RevokesAllActiveSessionsAndThrows()
    {
        var user = new UserBuilder().Build();
        var reusedToken = new RefreshToken
        {
            UserId = user.Id,
            User = user,
            TokenHash = "existing-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };
        var otherActiveToken = new RefreshToken
        {
            UserId = user.Id,
            User = user,
            TokenHash = "other-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        var context = MockDbContextFactory.Create(refreshTokens: [reusedToken, otherActiveToken]);
        var handler = new RefreshTokenHandler(
            context, CreateJwtTokenService("existing-hash"), CreateCurrentUserService(), NullLogger<RefreshTokenHandler>.Instance);

        var act = async () => await handler.Handle(new RefreshTokenCommand("incoming-token"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*reuse detected*");
        otherActiveToken.IsRevoked.Should().BeTrue();
    }
}

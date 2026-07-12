using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.Commands.Login;
using Renty.Server.Domain.Entities;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Auth.Commands.Login;

public sealed class LoginHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    private static IJwtTokenService CreateJwtTokenService()
    {
        var jwtTokenService = Substitute.For<IJwtTokenService>();
        jwtTokenService.GenerateAccessToken(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<IReadOnlyCollection<string>>())
            .Returns(("access-token", DateTime.UtcNow.AddMinutes(15)));
        jwtTokenService.GenerateRefreshToken().Returns("refresh-token");
        jwtTokenService.HashToken(Arg.Any<string>()).Returns("hashed-refresh-token");
        jwtTokenService.RefreshTokenLifetime.Returns(TimeSpan.FromDays(7));
        return jwtTokenService;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        var role = new RoleBuilder().WithName("Customer").Build();
        var user = new UserBuilder().WithEmail("jane@example.com").WithIsActive(true).Build();
        user.PasswordHash = "hashed-password";
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role });

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("correct-password", "hashed-password").Returns(true);

        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new LoginHandler(
            context, passwordHasher, CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<LoginHandler>.Instance);

        var result = await handler.Handle(new LoginCommand("jane@example.com", "correct-password"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        user.LastLoginAt.Should().NotBeNull();
        user.FailedLoginCount.Should().Be(0);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsUnauthorizedException()
    {
        var context = MockDbContextFactory.Create();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var handler = new LoginHandler(
            context, passwordHasher, CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<LoginHandler>.Instance);

        var act = async () => await handler.Handle(new LoginCommand("nobody@example.com", "password"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedException()
    {
        var user = new UserBuilder().WithEmail("jane@example.com").WithIsActive(false).Build();
        var context = MockDbContextFactory.Create(users: [user]);
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var handler = new LoginHandler(
            context, passwordHasher, CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<LoginHandler>.Instance);

        var act = async () => await handler.Handle(new LoginCommand("jane@example.com", "password"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*deactivated*");
    }

    [Fact]
    public async Task Handle_LockedOutUser_ThrowsUnauthorizedException()
    {
        var user = new UserBuilder().WithEmail("jane@example.com").WithIsActive(true).Build();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        var context = MockDbContextFactory.Create(users: [user]);
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var handler = new LoginHandler(
            context, passwordHasher, CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<LoginHandler>.Instance);

        var act = async () => await handler.Handle(new LoginCommand("jane@example.com", "password"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*locked*");
    }

    [Fact]
    public async Task Handle_WrongPassword_IncrementsFailedLoginCountAndThrows()
    {
        var user = new UserBuilder().WithEmail("jane@example.com").WithIsActive(true).Build();
        user.PasswordHash = "hashed-password";
        user.FailedLoginCount = 0;

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new LoginHandler(
            context, passwordHasher, CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<LoginHandler>.Instance);

        var act = async () => await handler.Handle(new LoginCommand("jane@example.com", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        user.FailedLoginCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_FifthWrongPassword_LocksAccount()
    {
        var user = new UserBuilder().WithEmail("jane@example.com").WithIsActive(true).Build();
        user.PasswordHash = "hashed-password";
        user.FailedLoginCount = 4;

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new LoginHandler(
            context, passwordHasher, CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<LoginHandler>.Instance);

        var act = async () => await handler.Handle(new LoginCommand("jane@example.com", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        user.FailedLoginCount.Should().Be(5);
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd!.Value.Should().BeAfter(DateTime.UtcNow);
    }
}

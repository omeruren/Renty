using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.Commands.ChangePassword;
using Renty.Server.Domain.Entities;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService(Guid? userId)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesPasswordAndRevokesActiveTokens()
    {
        var user = new UserBuilder().Build();
        user.PasswordHash = "old-hash";
        var activeToken = new RefreshToken { UserId = user.Id, TokenHash = "abc", ExpiresAt = DateTime.UtcNow.AddDays(1), IsRevoked = false };

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("current-password", "old-hash").Returns(true);
        passwordHasher.Hash("New-Str0ng!Pass").Returns("new-hash");

        var context = MockDbContextFactory.Create(users: [user], refreshTokens: [activeToken]);
        var handler = new ChangePasswordHandler(
            context, passwordHasher, CreateCurrentUserService(user.Id), NullLogger<ChangePasswordHandler>.Instance);

        await handler.Handle(new ChangePasswordCommand("current-password", "New-Str0ng!Pass"), CancellationToken.None);

        user.PasswordHash.Should().Be("new-hash");
        activeToken.IsRevoked.Should().BeTrue();
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IncorrectCurrentPassword_ThrowsUnauthorizedException()
    {
        var user = new UserBuilder().Build();
        user.PasswordHash = "old-hash";

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new ChangePasswordHandler(
            context, passwordHasher, CreateCurrentUserService(user.Id), NullLogger<ChangePasswordHandler>.Instance);

        var act = async () => await handler.Handle(new ChangePasswordCommand("wrong", "New-Str0ng!Pass"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_NoSignedInUser_ThrowsUnauthorizedException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new ChangePasswordHandler(
            context, Substitute.For<IPasswordHasher>(), CreateCurrentUserService(null), NullLogger<ChangePasswordHandler>.Instance);

        var act = async () => await handler.Handle(new ChangePasswordCommand("current", "New-Str0ng!Pass"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}

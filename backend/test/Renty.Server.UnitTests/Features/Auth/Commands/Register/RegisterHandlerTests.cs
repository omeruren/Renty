using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.Commands.Register;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Auth.Commands.Register;

public sealed class RegisterHandlerTests
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

    private static IPasswordHasher CreatePasswordHasher()
    {
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        return passwordHasher;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsAuthResponseAndCreatesUser()
    {
        var customerRole = new RoleBuilder().WithName("Customer").Build();
        var context = MockDbContextFactory.Create(roles: [customerRole]);
        var handler = new RegisterHandler(
            context, CreatePasswordHasher(), CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<RegisterHandler>.Instance);

        var command = new RegisterCommand("jane@example.com", "Str0ng!Pass", "Jane", "Doe", null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        context.Users.Received(1).Add(Arg.Is<Renty.Server.Domain.Entities.User>(u => u.Email == "jane@example.com"));
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ThrowsBusinessRuleException()
    {
        var existingUser = new UserBuilder().WithEmail("jane@example.com").Build();
        var customerRole = new RoleBuilder().WithName("Customer").Build();
        var context = MockDbContextFactory.Create(users: [existingUser], roles: [customerRole]);
        var handler = new RegisterHandler(
            context, CreatePasswordHasher(), CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<RegisterHandler>.Instance);

        var command = new RegisterCommand("jane@example.com", "Str0ng!Pass", "Jane", "Doe", null);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_DefaultCustomerRoleNotSeeded_ThrowsInvalidOperationException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new RegisterHandler(
            context, CreatePasswordHasher(), CreateJwtTokenService(), CreateCurrentUserService(), NullLogger<RegisterHandler>.Instance);

        var command = new RegisterCommand("jane@example.com", "Str0ng!Pass", "Jane", "Doe", null);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

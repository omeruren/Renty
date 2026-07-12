using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Profile.Commands.UpdateProfile;
using Renty.Server.Domain.Entities;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService(Guid? userId)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedProfileWithRoles()
    {
        var role = new RoleBuilder().WithName("Customer").Build();
        var user = new UserBuilder().Build();
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role });

        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new UpdateProfileHandler(context, CreateCurrentUserService(user.Id), NullLogger<UpdateProfileHandler>.Instance);
        var command = new UpdateProfileCommand("Jane", "Doe", "555-9999", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
        result.Roles.Should().Contain("Customer");
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoSignedInUser_ThrowsUnauthorizedException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new UpdateProfileHandler(context, CreateCurrentUserService(null), NullLogger<UpdateProfileHandler>.Instance);
        var command = new UpdateProfileCommand("Jane", "Doe", null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new UpdateProfileHandler(context, CreateCurrentUserService(Guid.NewGuid()), NullLogger<UpdateProfileHandler>.Instance);
        var command = new UpdateProfileCommand("Jane", "Doe", null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

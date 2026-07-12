using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Users.Commands.AssignRoles;
using Renty.Server.Domain.Entities;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Users.Commands.AssignRoles;

public sealed class AssignRolesHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService(Guid? userId = null)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId ?? Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_AssignsRoles()
    {
        var user = new UserBuilder().Build();
        var role = new RoleBuilder().WithName("Manager").Build();
        var context = MockDbContextFactory.Create(users: [user], roles: [role]);
        var handler = new AssignRolesHandler(context, CreateCurrentUserService(), NullLogger<AssignRolesHandler>.Instance);

        await handler.Handle(new AssignRolesCommand(user.Id, [role.Id]), CancellationToken.None);

        context.UserRoles.Received(1).Add(Arg.Is<UserRole>(ur => ur.UserId == user.Id && ur.RoleId == role.Id));
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownUser_ThrowsNotFoundException()
    {
        var role = new RoleBuilder().Build();
        var context = MockDbContextFactory.Create(roles: [role]);
        var handler = new AssignRolesHandler(context, CreateCurrentUserService(), NullLogger<AssignRolesHandler>.Instance);

        var act = async () => await handler.Handle(new AssignRolesCommand(Guid.NewGuid(), [role.Id]), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_UnknownRole_ThrowsNotFoundException()
    {
        var user = new UserBuilder().Build();
        var context = MockDbContextFactory.Create(users: [user]);
        var handler = new AssignRolesHandler(context, CreateCurrentUserService(), NullLogger<AssignRolesHandler>.Instance);

        var act = async () => await handler.Handle(new AssignRolesCommand(user.Id, [Guid.NewGuid()]), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_RemovingOwnAdminRole_ThrowsBusinessRuleException()
    {
        var user = new UserBuilder().Build();
        var adminRole = new RoleBuilder().WithName("Admin").Build();
        var managerRole = new RoleBuilder().WithName("Manager").Build();
        var existingUserRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id, User = user, Role = adminRole };

        var context = MockDbContextFactory.Create(
            users: [user], roles: [adminRole, managerRole], userRoles: [existingUserRole]);
        var handler = new AssignRolesHandler(context, CreateCurrentUserService(user.Id), NullLogger<AssignRolesHandler>.Instance);

        var act = async () => await handler.Handle(new AssignRolesCommand(user.Id, [managerRole.Id]), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*own Admin role*");
    }
}

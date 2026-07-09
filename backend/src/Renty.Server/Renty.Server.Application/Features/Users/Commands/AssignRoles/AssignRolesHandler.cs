using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Users.Commands.AssignRoles;

public sealed class AssignRolesHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<AssignRolesHandler> logger) : IRequestHandler<AssignRolesCommand>
{
    private const string AdminRoleName = "Admin";

    public async Task Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var roles = await context.Roles
            .Where(r => request.RoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        var foundRoleIds = roles.Select(r => r.Id).ToHashSet();
        var missingRoleIds = request.RoleIds.Distinct().Where(id => !foundRoleIds.Contains(id)).ToList();

        if (missingRoleIds.Count > 0)
            throw new NotFoundException(nameof(Role), missingRoleIds[0]);

        if (currentUserService.UserId == request.UserId && !roles.Any(r => r.Name == AdminRoleName))
        {
            var isCurrentlyAdmin = await context.UserRoles
                .AnyAsync(ur => ur.UserId == request.UserId && ur.Role.Name == AdminRoleName, cancellationToken);

            if (isCurrentlyAdmin)
                throw new BusinessRuleException("You cannot remove your own Admin role.");
        }

        var existingUserRoles = context.UserRoles.Where(ur => ur.UserId == request.UserId);
        context.UserRoles.RemoveRange(existingUserRoles);

        foreach (var roleId in request.RoleIds.Distinct())
            context.UserRoles.Add(new UserRole { UserId = request.UserId, RoleId = roleId });

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(User),
            EntityId = user.Id.ToString(),
            Action = AuditAction.RoleAssigned,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Roles updated for user {UserId}", user.Id);
    }
}

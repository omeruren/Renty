using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<DeactivateUserHandler> logger) : IRequestHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        user.IsActive = false;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(User),
            EntityId = user.Id.ToString(),
            Action = AuditAction.Update,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} deactivated", user.Id);
    }
}

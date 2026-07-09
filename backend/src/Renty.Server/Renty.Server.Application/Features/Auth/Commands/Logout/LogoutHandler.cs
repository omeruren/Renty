using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Auth.Commands.Logout;

public sealed class LogoutHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var activeTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(User),
            EntityId = userId.ToString(),
            Action = AuditAction.Logout,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} logged out", userId);
    }
}

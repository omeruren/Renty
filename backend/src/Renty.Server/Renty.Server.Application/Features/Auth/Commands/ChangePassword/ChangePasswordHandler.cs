using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    ILogger<ChangePasswordHandler> logger) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect.");

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);

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
            Action = AuditAction.PasswordChange,
            UserId = userId,
            UserEmail = user.Email,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} changed their password", userId);
    }
}

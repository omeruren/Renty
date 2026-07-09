using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenHandler(
    IApplicationDbContext context,
    IJwtTokenService jwtTokenService,
    ICurrentUserService currentUserService,
    ILogger<RefreshTokenHandler> logger) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var incomingHash = jwtTokenService.HashToken(request.RefreshToken);

        var token = await context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.TokenHash == incomingHash, cancellationToken);

        if (token is null)
            throw new UnauthorizedException("Invalid refresh token.");

        if (token.IsRevoked)
        {
            logger.LogError(
                "Refresh token reuse detected for user {UserId}. Revoking all active sessions.",
                token.UserId);

            var activeTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == token.UserId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var activeToken in activeTokens)
            {
                activeToken.IsRevoked = true;
                activeToken.RevokedAt = DateTime.UtcNow;
            }

            context.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(RefreshToken),
                EntityId = token.Id.ToString(),
                Action = AuditAction.FailedLogin,
                UserId = token.UserId,
                UserEmail = token.User.Email,
                Timestamp = DateTime.UtcNow,
                IpAddress = currentUserService.IpAddress
            });

            await context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Security alert: token reuse detected. Please log in again.");
        }

        if (token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token has expired.");

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        var roles = token.User.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var permissions = token.User.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToArray();

        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        var newHash = jwtTokenService.HashToken(newRefreshToken);
        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(
            token.User.Id,
            token.User.Email,
            $"{token.User.FirstName} {token.User.LastName}",
            roles,
            permissions);

        token.ReplacedByTokenHash = newHash;

        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.Add(jwtTokenService.RefreshTokenLifetime),
            UserId = token.UserId
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Refresh token rotated for user {UserId}", token.UserId);

        return new AuthResponse(accessToken, newRefreshToken, expiresAt);
    }
}

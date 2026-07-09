using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ICurrentUserService currentUserService,
    ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, AuthResponse>
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            await LogFailedAttemptAsync(null, request.Email, cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            await LogFailedAttemptAsync(user.Id, request.Email, cancellationToken);
            throw new UnauthorizedException("This account has been deactivated.");
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            await LogFailedAttemptAsync(user.Id, request.Email, cancellationToken);
            throw new UnauthorizedException("This account is locked. Please try again later.");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;

            if (user.FailedLoginCount >= MaxFailedLoginAttempts)
                user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);

            await context.SaveChangesAsync(cancellationToken);
            await LogFailedAttemptAsync(user.Id, request.Email, cancellationToken);

            throw new UnauthorizedException("Invalid email or password.");
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToArray();

        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, $"{user.FirstName} {user.LastName}", roles, permissions);

        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = jwtTokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.Add(jwtTokenService.RefreshTokenLifetime),
            UserId = user.Id
        });

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(User),
            EntityId = user.Id.ToString(),
            Action = AuditAction.Login,
            UserId = user.Id,
            UserEmail = user.Email,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} logged in", user.Id);

        return new AuthResponse(accessToken, refreshToken, expiresAt);
    }

    private async Task LogFailedAttemptAsync(Guid? userId, string email, CancellationToken cancellationToken)
    {
        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(User),
            EntityId = userId?.ToString() ?? email,
            Action = AuditAction.FailedLogin,
            UserId = userId,
            UserEmail = email,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        logger.LogWarning("Failed login attempt for {Email}", email);

        await context.SaveChangesAsync(cancellationToken);
    }
}

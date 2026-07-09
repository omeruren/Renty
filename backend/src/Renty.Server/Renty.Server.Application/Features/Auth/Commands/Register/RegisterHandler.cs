using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Auth.Commands.Register;

public sealed class RegisterHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ICurrentUserService currentUserService,
    ILogger<RegisterHandler> logger) : IRequestHandler<RegisterCommand, AuthResponse>
{
    private const string DefaultRoleName = "Customer";

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
            throw new BusinessRuleException("A user with this email address already exists.");

        var customerRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == DefaultRoleName, cancellationToken);

        if (customerRole is null)
            throw new InvalidOperationException($"Default '{DefaultRoleName}' role is not seeded.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            EmailConfirmed = false
        };

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = customerRole.Id });

        context.Users.Add(user);

        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, $"{user.FirstName} {user.LastName}", [DefaultRoleName], []);

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
            Action = AuditAction.Create,
            UserId = user.Id,
            UserEmail = user.Email,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} registered", user.Id);

        return new AuthResponse(accessToken, refreshToken, expiresAt);
    }
}

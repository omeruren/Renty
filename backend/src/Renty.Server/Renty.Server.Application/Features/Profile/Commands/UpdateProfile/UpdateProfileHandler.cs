using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Profile.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<UpdateProfileHandler> logger) : IRequestHandler<UpdateProfileCommand, ProfileResponse>
{
    public async Task<ProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var user = await context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(User),
            EntityId = user.Id.ToString(),
            Action = AuditAction.Update,
            UserId = userId,
            UserEmail = user.Email,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} updated their profile", userId);

        return new ProfileResponse(
            user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, user.DateOfBirth,
            user.UserRoles.Select(ur => ur.Role.Name).ToArray(),
            user.CreatedAt);
    }
}

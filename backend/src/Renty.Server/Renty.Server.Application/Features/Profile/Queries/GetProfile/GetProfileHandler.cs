using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Profile.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Profile.Queries.GetProfile;

public sealed class GetProfileHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetProfileQuery, ProfileResponse>
{
    public async Task<ProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new ProfileResponse(
                u.Id, u.Email, u.FirstName, u.LastName, u.PhoneNumber, u.DateOfBirth,
                u.UserRoles.Select(ur => ur.Role.Name).ToArray(),
                u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);
    }
}

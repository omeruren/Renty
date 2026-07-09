using System.Linq.Expressions;
using Renty.Server.Application.Features.Users.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Users.Mappings;

public static class UserProjections
{
    public static Expression<Func<User, UserResponse>> ToResponse { get; } =
        u => new UserResponse(
            u.Id, u.Email, u.FirstName, u.LastName, u.PhoneNumber, u.DateOfBirth,
            u.IsActive, u.EmailConfirmed, u.LastLoginAt,
            u.UserRoles.Select(ur => ur.Role.Name).ToArray(),
            u.CreatedAt);

    public static Expression<Func<User, UserListResponse>> ToListResponse { get; } =
        u => new UserListResponse(
            u.Id, u.Email, u.FirstName, u.LastName, u.IsActive,
            u.UserRoles.Select(ur => ur.Role.Name).ToArray());
}

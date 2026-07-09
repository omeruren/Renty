namespace Renty.Server.Application.Features.Users.DTOs;

public sealed record UserListResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    string[] Roles);

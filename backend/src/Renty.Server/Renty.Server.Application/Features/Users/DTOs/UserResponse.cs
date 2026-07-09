namespace Renty.Server.Application.Features.Users.DTOs;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    bool IsActive,
    bool EmailConfirmed,
    DateTime? LastLoginAt,
    string[] Roles,
    DateTime CreatedAt);

namespace Renty.Server.Application.Features.Profile.DTOs;

public sealed record ProfileResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string[] Roles,
    DateTime CreatedAt);

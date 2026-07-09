namespace Renty.Server.Application.Features.Locations.DTOs;

public sealed record LocationResponse(
    Guid Id,
    string Name,
    string Address,
    string City,
    string? District,
    string? PhoneNumber,
    string? Email,
    double? Latitude,
    double? Longitude,
    bool IsActive,
    DateTime CreatedAt);

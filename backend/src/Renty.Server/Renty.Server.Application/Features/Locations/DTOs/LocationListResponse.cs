namespace Renty.Server.Application.Features.Locations.DTOs;

public sealed record LocationListResponse(Guid Id, string Name, string City, bool IsActive);

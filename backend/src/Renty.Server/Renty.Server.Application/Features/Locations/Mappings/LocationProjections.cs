using System.Linq.Expressions;
using Renty.Server.Application.Features.Locations.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Locations.Mappings;

public static class LocationProjections
{
    public static Expression<Func<Location, LocationResponse>> ToResponse { get; } =
        l => new LocationResponse(
            l.Id, l.Name, l.Address, l.City, l.District, l.PhoneNumber, l.Email,
            l.Latitude, l.Longitude, l.IsActive, l.CreatedAt);

    public static Expression<Func<Location, LocationListResponse>> ToListResponse { get; } =
        l => new LocationListResponse(l.Id, l.Name, l.City, l.IsActive);
}

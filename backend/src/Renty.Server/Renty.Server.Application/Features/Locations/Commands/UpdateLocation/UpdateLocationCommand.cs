using MediatR;
using Renty.Server.Application.Features.Locations.DTOs;

namespace Renty.Server.Application.Features.Locations.Commands.UpdateLocation;

public sealed record UpdateLocationCommand(
    Guid Id,
    string Name,
    string Address,
    string City,
    string? District,
    string? PhoneNumber,
    string? Email,
    double? Latitude,
    double? Longitude,
    bool IsActive) : IRequest<LocationResponse>;

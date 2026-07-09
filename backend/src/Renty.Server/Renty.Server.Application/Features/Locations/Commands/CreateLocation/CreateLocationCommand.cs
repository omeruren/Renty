using MediatR;
using Renty.Server.Application.Features.Locations.DTOs;

namespace Renty.Server.Application.Features.Locations.Commands.CreateLocation;

public sealed record CreateLocationCommand(
    string Name,
    string Address,
    string City,
    string? District,
    string? PhoneNumber,
    string? Email,
    double? Latitude,
    double? Longitude) : IRequest<LocationResponse>;

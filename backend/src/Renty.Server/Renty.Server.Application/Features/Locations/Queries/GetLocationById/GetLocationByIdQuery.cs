using MediatR;
using Renty.Server.Application.Features.Locations.DTOs;

namespace Renty.Server.Application.Features.Locations.Queries.GetLocationById;

public sealed record GetLocationByIdQuery(Guid Id) : IRequest<LocationResponse>;

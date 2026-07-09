using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Locations.DTOs;

namespace Renty.Server.Application.Features.Locations.Queries.GetAllLocations;

public sealed record GetAllLocationsQuery(
    int Page = 1,
    int PageSize = 10,
    string? City = null,
    bool? IsActive = null) : IRequest<PagedResponse<LocationListResponse>>;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Locations.DTOs;
using Renty.Server.Application.Features.Locations.Mappings;

namespace Renty.Server.Application.Features.Locations.Queries.GetAllLocations;

public sealed class GetAllLocationsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllLocationsQuery, PagedResponse<LocationListResponse>>
{
    public async Task<PagedResponse<LocationListResponse>> Handle(
        GetAllLocationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Locations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(l => l.City.Contains(request.City));

        if (request.IsActive.HasValue)
            query = query.Where(l => l.IsActive == request.IsActive.Value);

        query = query.OrderBy(l => l.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(LocationProjections.ToListResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<LocationListResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}

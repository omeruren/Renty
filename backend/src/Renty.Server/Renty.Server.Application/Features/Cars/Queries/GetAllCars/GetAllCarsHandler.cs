using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Application.Features.Cars.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Cars.Queries.GetAllCars;

public sealed class GetAllCarsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllCarsQuery, PagedResponse<CarListResponse>>
{
    public async Task<PagedResponse<CarListResponse>> Handle(
        GetAllCarsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Cars.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.BrandId.HasValue)
            query = query.Where(c => c.BrandId == request.BrandId.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(c => c.DailyPrice >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(c => c.DailyPrice <= request.MaxPrice.Value);

        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(CarProjections.ToListResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<CarListResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }

    private static IQueryable<Car> ApplySorting(IQueryable<Car> query, string sortBy, string sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "year" => descending ? query.OrderByDescending(c => c.Year) : query.OrderBy(c => c.Year),
            "dailyprice" => descending
                ? query.OrderByDescending(c => c.DailyPrice)
                : query.OrderBy(c => c.DailyPrice),
            _ => descending
                ? query.OrderByDescending(c => c.LicensePlate)
                : query.OrderBy(c => c.LicensePlate)
        };
    }
}

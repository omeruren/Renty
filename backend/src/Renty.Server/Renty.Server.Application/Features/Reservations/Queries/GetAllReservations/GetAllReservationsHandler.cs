using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Application.Features.Reservations.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Reservations.Queries.GetAllReservations;

public sealed class GetAllReservationsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllReservationsQuery, PagedResponse<ReservationListResponse>>
{
    public async Task<PagedResponse<ReservationListResponse>> Handle(
        GetAllReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Reservations.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.CarId.HasValue)
            query = query.Where(r => r.CarId == request.CarId.Value);

        if (request.UserId.HasValue)
            query = query.Where(r => r.UserId == request.UserId.Value);

        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ReservationProjections.ToListResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<ReservationListResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }

    private static IQueryable<Reservation> ApplySorting(IQueryable<Reservation> query, string sortBy, string sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "enddate" => descending ? query.OrderByDescending(r => r.EndDate) : query.OrderBy(r => r.EndDate),
            "totalprice" => descending
                ? query.OrderByDescending(r => r.TotalPrice)
                : query.OrderBy(r => r.TotalPrice),
            "createdat" => descending
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt),
            _ => descending ? query.OrderByDescending(r => r.StartDate) : query.OrderBy(r => r.StartDate)
        };
    }
}

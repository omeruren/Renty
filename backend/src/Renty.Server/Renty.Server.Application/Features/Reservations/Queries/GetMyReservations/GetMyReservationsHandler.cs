using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Application.Features.Reservations.Mappings;

namespace Renty.Server.Application.Features.Reservations.Queries.GetMyReservations;

public sealed class GetMyReservationsHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetMyReservationsQuery, PagedResponse<ReservationListResponse>>
{
    public async Task<PagedResponse<ReservationListResponse>> Handle(
        GetMyReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("A signed-in user is required.");

        var query = context.Reservations.AsNoTracking().Where(r => r.UserId == userId);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        query = query.OrderByDescending(r => r.StartDate);

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
}

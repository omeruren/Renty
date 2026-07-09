using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Constants;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reports.DTOs;

namespace Renty.Server.Application.Features.Reports.Queries.GetRevenueReport;

public sealed class GetRevenueReportHandler(IApplicationDbContext context)
    : IRequestHandler<GetRevenueReportQuery, RevenueReportResponse>
{
    public async Task<RevenueReportResponse> Handle(
        GetRevenueReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.From ?? DateTime.MinValue;
        var to = request.To ?? DateTime.MaxValue;

        var reservations = await context.Reservations
            .AsNoTracking()
            .Where(r => ReservationStatuses.Revenue.Contains(r.Status)
                && r.StartDate >= from
                && r.StartDate <= to)
            .Select(r => r.TotalPrice)
            .ToListAsync(cancellationToken);

        var totalRevenue = reservations.Sum();
        var reservationCount = reservations.Count;

        return new RevenueReportResponse(
            request.From,
            request.To,
            totalRevenue,
            reservationCount,
            reservationCount > 0 ? totalRevenue / reservationCount : 0);
    }
}

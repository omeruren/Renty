namespace Renty.Server.Application.Features.Reports.DTOs;

public sealed record RevenueReportResponse(
    DateTime? From,
    DateTime? To,
    decimal TotalRevenue,
    int ReservationCount,
    decimal AverageReservationValue);

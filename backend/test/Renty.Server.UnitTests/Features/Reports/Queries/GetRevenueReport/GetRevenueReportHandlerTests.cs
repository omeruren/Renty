using FluentAssertions;
using Renty.Server.Application.Features.Reports.Queries.GetRevenueReport;
using Renty.Server.Domain.Enums;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Reports.Queries.GetRevenueReport;

public sealed class GetRevenueReportHandlerTests
{
    [Fact]
    public async Task Handle_OnlyCountsActiveAndCompletedReservations()
    {
        var counted1 = new ReservationBuilder().WithStatus(ReservationStatus.Completed).Build();
        counted1.TotalPrice = 1000m;
        var counted2 = new ReservationBuilder().WithStatus(ReservationStatus.Active).Build();
        counted2.TotalPrice = 500m;
        var ignoredPending = new ReservationBuilder().WithStatus(ReservationStatus.Pending).Build();
        ignoredPending.TotalPrice = 9999m;
        var ignoredCancelled = new ReservationBuilder().WithStatus(ReservationStatus.Cancelled).Build();
        ignoredCancelled.TotalPrice = 9999m;

        var context = MockDbContextFactory.Create(reservations: [counted1, counted2, ignoredPending, ignoredCancelled]);
        var handler = new GetRevenueReportHandler(context);

        var result = await handler.Handle(new GetRevenueReportQuery(null, null), CancellationToken.None);

        result.TotalRevenue.Should().Be(1500m);
        result.ReservationCount.Should().Be(2);
        result.AverageReservationValue.Should().Be(750m);
    }

    [Fact]
    public async Task Handle_FiltersByDateRange()
    {
        var inRange = new ReservationBuilder().WithStatus(ReservationStatus.Completed)
            .WithDates(new DateTime(2026, 3, 15), new DateTime(2026, 3, 18)).Build();
        inRange.TotalPrice = 300m;
        var outOfRange = new ReservationBuilder().WithStatus(ReservationStatus.Completed)
            .WithDates(new DateTime(2026, 6, 1), new DateTime(2026, 6, 3)).Build();
        outOfRange.TotalPrice = 700m;

        var context = MockDbContextFactory.Create(reservations: [inRange, outOfRange]);
        var handler = new GetRevenueReportHandler(context);

        var result = await handler.Handle(
            new GetRevenueReportQuery(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)), CancellationToken.None);

        result.TotalRevenue.Should().Be(300m);
        result.ReservationCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoMatchingReservations_ReturnsZeroAverage()
    {
        var context = MockDbContextFactory.Create();
        var handler = new GetRevenueReportHandler(context);

        var result = await handler.Handle(new GetRevenueReportQuery(null, null), CancellationToken.None);

        result.TotalRevenue.Should().Be(0);
        result.ReservationCount.Should().Be(0);
        result.AverageReservationValue.Should().Be(0);
    }
}

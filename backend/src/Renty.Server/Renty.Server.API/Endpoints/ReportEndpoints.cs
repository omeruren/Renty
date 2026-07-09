using MediatR;
using Renty.Server.Application.Features.Reports.DTOs;
using Renty.Server.Application.Features.Reports.Queries.GetRevenueReport;

namespace Renty.Server.API.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports").RequireAuthorization("CanViewReports");

        group.MapGet("/revenue", GetRevenueReport)
            .WithName("GetRevenueReport")
            .Produces<RevenueReportResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> GetRevenueReport(
        ISender sender,
        CancellationToken cancellationToken,
        DateTime? from = null,
        DateTime? to = null)
    {
        var result = await sender.Send(new GetRevenueReportQuery(from, to), cancellationToken);
        return Results.Ok(result);
    }
}

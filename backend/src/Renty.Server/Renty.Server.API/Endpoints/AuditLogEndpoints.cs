using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.AuditLogs.DTOs;
using Renty.Server.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

namespace Renty.Server.API.Endpoints;

public static class AuditLogEndpoints
{
    public static void MapAuditLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/audit-logs").WithTags("Audit Logs").RequireAuthorization("AdminOnly");

        group.MapGet("/", GetAllAuditLogs)
            .WithName("GetAllAuditLogs")
            .Produces<PagedResponse<AuditLogResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAllAuditLogs(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        string? entityName = null,
        Guid? userId = null)
    {
        var result = await sender.Send(new GetAllAuditLogsQuery(page, pageSize, entityName, userId), cancellationToken);
        return Results.Ok(result);
    }
}

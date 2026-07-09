using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.AuditLogs.DTOs;
using Renty.Server.Application.Features.AuditLogs.Mappings;

namespace Renty.Server.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public sealed class GetAllAuditLogsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllAuditLogsQuery, PagedResponse<AuditLogResponse>>
{
    public async Task<PagedResponse<AuditLogResponse>> Handle(
        GetAllAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(a => a.EntityName == request.EntityName);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        query = query.OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(AuditLogProjections.ToResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLogResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}

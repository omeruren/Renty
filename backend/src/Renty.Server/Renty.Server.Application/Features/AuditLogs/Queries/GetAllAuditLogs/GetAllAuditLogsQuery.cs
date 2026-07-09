using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.AuditLogs.DTOs;

namespace Renty.Server.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public sealed record GetAllAuditLogsQuery(
    int Page = 1,
    int PageSize = 10,
    string? EntityName = null,
    Guid? UserId = null) : IRequest<PagedResponse<AuditLogResponse>>;

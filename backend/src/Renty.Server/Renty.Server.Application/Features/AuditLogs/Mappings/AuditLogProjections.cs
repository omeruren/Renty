using System.Linq.Expressions;
using Renty.Server.Application.Features.AuditLogs.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.AuditLogs.Mappings;

public static class AuditLogProjections
{
    public static Expression<Func<AuditLog, AuditLogResponse>> ToResponse { get; } =
        a => new AuditLogResponse(
            a.Id, a.EntityName, a.EntityId, a.Action, a.OldValues, a.NewValues,
            a.UserId, a.UserEmail, a.Timestamp, a.IpAddress);
}

using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.AuditLogs.DTOs;

public sealed record AuditLogResponse(
    Guid Id,
    string EntityName,
    string EntityId,
    AuditAction Action,
    string? OldValues,
    string? NewValues,
    Guid? UserId,
    string? UserEmail,
    DateTime Timestamp,
    string? IpAddress);

using Renty.Server.Domain.Common;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public AuditAction Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}

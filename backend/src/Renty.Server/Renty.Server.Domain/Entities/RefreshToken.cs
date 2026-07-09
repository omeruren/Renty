using Renty.Server.Domain.Common;

namespace Renty.Server.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public Guid UserId { get; set; }

    public User User { get; set; } = default!;
}

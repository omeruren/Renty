using Renty.Server.Domain.Common;

namespace Renty.Server.Domain.Entities;

public sealed class Permission : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Module { get; set; } = default!;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

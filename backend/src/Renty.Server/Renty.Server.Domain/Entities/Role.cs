using Renty.Server.Domain.Common;

namespace Renty.Server.Domain.Entities;

public sealed class Role : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

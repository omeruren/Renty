using Renty.Server.Domain.Entities;

namespace Renty.Server.Persistence.Seed;

public static class SeedData
{
    private static readonly DateTime SeedTimestamp = new(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "System";

    public static readonly Guid AdminRoleId = Guid.Parse("11111111-0000-0000-0000-000000000001");
    public static readonly Guid ManagerRoleId = Guid.Parse("11111111-0000-0000-0000-000000000002");
    public static readonly Guid CustomerRoleId = Guid.Parse("11111111-0000-0000-0000-000000000003");

    public static readonly Guid CarsCreateId = Guid.Parse("22222222-0000-0000-0000-000000000001");
    public static readonly Guid CarsReadId = Guid.Parse("22222222-0000-0000-0000-000000000002");
    public static readonly Guid CarsUpdateId = Guid.Parse("22222222-0000-0000-0000-000000000003");
    public static readonly Guid CarsDeleteId = Guid.Parse("22222222-0000-0000-0000-000000000004");
    public static readonly Guid ReservationsCreateId = Guid.Parse("22222222-0000-0000-0000-000000000005");
    public static readonly Guid ReservationsReadId = Guid.Parse("22222222-0000-0000-0000-000000000006");
    public static readonly Guid ReservationsUpdateId = Guid.Parse("22222222-0000-0000-0000-000000000007");
    public static readonly Guid ReservationsCancelId = Guid.Parse("22222222-0000-0000-0000-000000000008");
    public static readonly Guid UsersReadId = Guid.Parse("22222222-0000-0000-0000-000000000009");
    public static readonly Guid UsersManageId = Guid.Parse("22222222-0000-0000-0000-000000000010");
    public static readonly Guid LocationsManageId = Guid.Parse("22222222-0000-0000-0000-000000000011");
    public static readonly Guid PricingManageId = Guid.Parse("22222222-0000-0000-0000-000000000012");
    public static readonly Guid ReportsViewId = Guid.Parse("22222222-0000-0000-0000-000000000013");
    public static readonly Guid AuditLogsViewId = Guid.Parse("22222222-0000-0000-0000-000000000014");

    public static Role[] Roles =>
    [
        new()
        {
            Id = AdminRoleId,
            Name = "Admin",
            Description = "Full system access",
            IsSystemRole = true,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        },
        new()
        {
            Id = ManagerRoleId,
            Name = "Manager",
            Description = "Fleet and reservation management",
            IsSystemRole = true,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        },
        new()
        {
            Id = CustomerRoleId,
            Name = "Customer",
            Description = "Standard customer access",
            IsSystemRole = true,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        }
    ];

    public static Permission[] Permissions =>
    [
        Permission(CarsCreateId, "Cars.Create", "Cars", "Create new cars"),
        Permission(CarsReadId, "Cars.Read", "Cars", "View car information"),
        Permission(CarsUpdateId, "Cars.Update", "Cars", "Update car information"),
        Permission(CarsDeleteId, "Cars.Delete", "Cars", "Delete cars"),
        Permission(ReservationsCreateId, "Reservations.Create", "Reservations", "Create reservations"),
        Permission(ReservationsReadId, "Reservations.Read", "Reservations", "View reservations"),
        Permission(ReservationsUpdateId, "Reservations.Update", "Reservations", "Update reservations"),
        Permission(ReservationsCancelId, "Reservations.Cancel", "Reservations", "Cancel reservations"),
        Permission(UsersReadId, "Users.Read", "Users", "View user information"),
        Permission(UsersManageId, "Users.Manage", "Users", "Manage user accounts"),
        Permission(LocationsManageId, "Locations.Manage", "Locations", "Manage locations"),
        Permission(PricingManageId, "Pricing.Manage", "Pricing", "Manage pricing rules"),
        Permission(ReportsViewId, "Reports.View", "Reports", "View system reports"),
        Permission(AuditLogsViewId, "AuditLogs.View", "AuditLogs", "View audit logs")
    ];

    public static RolePermission[] RolePermissions =>
    [
        // Admin: all permissions
        .. Permissions.Select(p => new RolePermission { RoleId = AdminRoleId, PermissionId = p.Id }),

        // Manager: Cars.*, Reservations.*, Locations.Manage, Reports.View
        RolePermission(ManagerRoleId, CarsCreateId),
        RolePermission(ManagerRoleId, CarsReadId),
        RolePermission(ManagerRoleId, CarsUpdateId),
        RolePermission(ManagerRoleId, CarsDeleteId),
        RolePermission(ManagerRoleId, ReservationsCreateId),
        RolePermission(ManagerRoleId, ReservationsReadId),
        RolePermission(ManagerRoleId, ReservationsUpdateId),
        RolePermission(ManagerRoleId, ReservationsCancelId),
        RolePermission(ManagerRoleId, LocationsManageId),
        RolePermission(ManagerRoleId, ReportsViewId),

        // Customer: Cars.Read, Reservations.Create/Read/Cancel
        RolePermission(CustomerRoleId, CarsReadId),
        RolePermission(CustomerRoleId, ReservationsCreateId),
        RolePermission(CustomerRoleId, ReservationsReadId),
        RolePermission(CustomerRoleId, ReservationsCancelId)
    ];

    private static Permission Permission(Guid id, string name, string module, string description) => new()
    {
        Id = id,
        Name = name,
        Module = module,
        Description = description,
        CreatedAt = SeedTimestamp,
        CreatedBy = SeedAuthor
    };

    private static RolePermission RolePermission(Guid roleId, Guid permissionId) => new()
    {
        RoleId = roleId,
        PermissionId = permissionId
    };
}

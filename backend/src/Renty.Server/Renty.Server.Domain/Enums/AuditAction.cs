namespace Renty.Server.Domain.Enums;

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Login,
    Logout,
    FailedLogin,
    PasswordChange,
    RoleAssigned,
    RoleRemoved
}

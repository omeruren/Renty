using Renty.Server.Application.Common.Interfaces;

namespace Renty.Server.Infrastructure.Services;

/// <summary>
/// Placeholder implementation until JWT authentication (Phase 3) provides a real HttpContext-based user.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
}

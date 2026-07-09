using MediatR;

namespace Renty.Server.Application.Features.Users.Commands.AssignRoles;

public sealed record AssignRolesCommand(Guid UserId, List<Guid> RoleIds) : IRequest;

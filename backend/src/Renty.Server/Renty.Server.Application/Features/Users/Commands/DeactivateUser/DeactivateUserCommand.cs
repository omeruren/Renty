using MediatR;

namespace Renty.Server.Application.Features.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid Id) : IRequest;

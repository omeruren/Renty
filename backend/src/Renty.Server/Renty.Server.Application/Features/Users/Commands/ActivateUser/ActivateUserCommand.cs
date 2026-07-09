using MediatR;

namespace Renty.Server.Application.Features.Users.Commands.ActivateUser;

public sealed record ActivateUserCommand(Guid Id) : IRequest;

using MediatR;

namespace Renty.Server.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand : IRequest;

using MediatR;
using Renty.Server.Application.Features.Auth.DTOs;

namespace Renty.Server.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

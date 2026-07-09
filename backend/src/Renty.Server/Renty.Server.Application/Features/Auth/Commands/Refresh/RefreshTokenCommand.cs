using MediatR;
using Renty.Server.Application.Features.Auth.DTOs;

namespace Renty.Server.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

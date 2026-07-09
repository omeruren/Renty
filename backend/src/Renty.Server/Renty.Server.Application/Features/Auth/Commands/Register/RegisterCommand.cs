using MediatR;
using Renty.Server.Application.Features.Auth.DTOs;

namespace Renty.Server.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest<AuthResponse>;

using MediatR;

namespace Renty.Server.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;

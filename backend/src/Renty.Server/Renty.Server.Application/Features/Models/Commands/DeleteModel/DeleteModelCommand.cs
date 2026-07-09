using MediatR;

namespace Renty.Server.Application.Features.Models.Commands.DeleteModel;

public sealed record DeleteModelCommand(Guid Id) : IRequest;

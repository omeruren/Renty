using MediatR;

namespace Renty.Server.Application.Features.Locations.Commands.DeleteLocation;

public sealed record DeleteLocationCommand(Guid Id) : IRequest;

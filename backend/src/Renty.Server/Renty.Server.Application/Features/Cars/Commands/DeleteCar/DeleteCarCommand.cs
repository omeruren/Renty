using MediatR;

namespace Renty.Server.Application.Features.Cars.Commands.DeleteCar;

public sealed record DeleteCarCommand(Guid Id) : IRequest;

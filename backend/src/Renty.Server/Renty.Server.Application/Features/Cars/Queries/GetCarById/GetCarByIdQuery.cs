using MediatR;
using Renty.Server.Application.Features.Cars.DTOs;

namespace Renty.Server.Application.Features.Cars.Queries.GetCarById;

public sealed record GetCarByIdQuery(Guid Id) : IRequest<CarResponse>;

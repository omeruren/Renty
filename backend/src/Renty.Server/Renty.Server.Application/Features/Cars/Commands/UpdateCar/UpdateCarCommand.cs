using MediatR;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Cars.Commands.UpdateCar;

public sealed record UpdateCarCommand(
    Guid Id,
    string Color,
    int Mileage,
    decimal DailyPrice,
    CarStatus Status,
    string? Description,
    string? ImageUrl,
    Guid LocationId) : IRequest<CarResponse>;

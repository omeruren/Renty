using MediatR;
using Renty.Server.Application.Features.Cars.DTOs;

namespace Renty.Server.Application.Features.Cars.Commands.CreateCar;

public sealed record CreateCarCommand(
    string LicensePlate,
    int Year,
    string Color,
    int Mileage,
    decimal DailyPrice,
    string? Description,
    string? ImageUrl,
    Guid BrandId,
    Guid ModelId,
    Guid LocationId) : IRequest<CarResponse>;

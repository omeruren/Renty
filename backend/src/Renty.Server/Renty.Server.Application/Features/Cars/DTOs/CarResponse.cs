using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Cars.DTOs;

public sealed record CarResponse(
    Guid Id,
    string LicensePlate,
    int Year,
    string Color,
    int Mileage,
    decimal DailyPrice,
    CarStatus Status,
    string? Description,
    string? ImageUrl,
    Guid BrandId,
    string BrandName,
    Guid ModelId,
    string ModelName,
    Guid LocationId,
    string LocationName,
    DateTime CreatedAt);

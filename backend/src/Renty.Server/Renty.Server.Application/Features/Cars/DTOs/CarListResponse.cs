using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Cars.DTOs;

public sealed record CarListResponse(
    Guid Id,
    string LicensePlate,
    int Year,
    string Color,
    decimal DailyPrice,
    CarStatus Status,
    string BrandName,
    string ModelName,
    string LocationName);

using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Models.DTOs;

public sealed record ModelListResponse(
    Guid Id,
    string Name,
    VehicleCategory Category,
    int SeatCount,
    TransmissionType TransmissionType,
    FuelType FuelType,
    Guid BrandId,
    string BrandName);

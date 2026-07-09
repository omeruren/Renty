using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.DTOs;

public sealed record ReservationResponse(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice,
    ReservationStatus Status,
    string? Notes,
    string? CancellationReason,
    DateTime? CancelledAt,
    Guid UserId,
    string UserName,
    string UserEmail,
    Guid CarId,
    string CarLicensePlate,
    string CarBrandName,
    string CarModelName,
    Guid PickupLocationId,
    string PickupLocationName,
    Guid ReturnLocationId,
    string ReturnLocationName,
    DateTime CreatedAt);

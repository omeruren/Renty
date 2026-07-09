using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.DTOs;

public sealed record ReservationListResponse(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice,
    ReservationStatus Status,
    Guid UserId,
    string UserName,
    Guid CarId,
    string CarLicensePlate,
    string PickupLocationName,
    string ReturnLocationName);

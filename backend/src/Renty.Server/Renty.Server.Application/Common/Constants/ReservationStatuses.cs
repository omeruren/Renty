using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Common.Constants;

public static class ReservationStatuses
{
    /// <summary>
    /// Statuses that occupy a car's calendar and block deletion, re-reservation, or overlap-free booking.
    /// </summary>
    public static readonly ReservationStatus[] Blocking =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed,
        ReservationStatus.Active
    ];
}

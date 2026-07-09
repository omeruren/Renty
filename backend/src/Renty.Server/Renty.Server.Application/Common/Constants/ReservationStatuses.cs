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

    /// <summary>
    /// Statuses representing realized or committed revenue (rental has started or finished).
    /// </summary>
    public static readonly ReservationStatus[] Revenue =
    [
        ReservationStatus.Active,
        ReservationStatus.Completed
    ];
}

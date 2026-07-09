using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Constants;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Common;

/// <summary>
/// Recomputes a car's status from its remaining reservations whenever a reservation
/// that was occupying the car's calendar (cancelled/completed/expired) stops doing so.
/// </summary>
public static class CarStatusReconciler
{
    public static async Task ReconcileAsync(
        IApplicationDbContext context,
        Guid carId,
        Guid excludeReservationId,
        CancellationToken cancellationToken)
    {
        var otherBlockingStatuses = await context.Reservations
            .Where(r => r.CarId == carId
                && r.Id != excludeReservationId
                && ReservationStatuses.Blocking.Contains(r.Status))
            .Select(r => r.Status)
            .ToListAsync(cancellationToken);

        var car = await context.Cars.FirstAsync(c => c.Id == carId, cancellationToken);

        car.Status = otherBlockingStatuses.Contains(ReservationStatus.Active)
            ? CarStatus.Rented
            : otherBlockingStatuses.Count > 0
                ? CarStatus.Reserved
                : CarStatus.Available;
    }
}

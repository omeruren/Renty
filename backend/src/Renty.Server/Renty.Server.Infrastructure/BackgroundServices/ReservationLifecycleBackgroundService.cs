using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reservations.Common;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Infrastructure.BackgroundServices;

/// <summary>
/// Drives the two reservation transitions that have no client-triggered endpoint:
/// Confirmed -> Active once the rental's start date arrives, and Pending -> Expired
/// for bookings that were never confirmed by staff before their start date.
/// </summary>
public sealed class ReservationLifecycleBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservationLifecycleBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing reservation lifecycle transitions.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var now = DateTime.UtcNow;
        var hasChanges = false;

        var startingReservations = await context.Reservations
            .Include(r => r.Car)
            .Where(r => r.Status == ReservationStatus.Confirmed && r.StartDate <= now)
            .ToListAsync(cancellationToken);

        foreach (var reservation in startingReservations)
        {
            reservation.Status = ReservationStatus.Active;
            reservation.Car.Status = CarStatus.Rented;
            logger.LogInformation("Reservation {ReservationId} activated", reservation.Id);
            hasChanges = true;
        }

        var noShowReservationIds = await context.Reservations
            .Where(r => r.Status == ReservationStatus.Pending && r.StartDate <= now)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        foreach (var reservationId in noShowReservationIds)
        {
            var reservation = await context.Reservations
                .FirstAsync(r => r.Id == reservationId, cancellationToken);

            reservation.Status = ReservationStatus.Expired;
            await CarStatusReconciler.ReconcileAsync(context, reservation.CarId, reservation.Id, cancellationToken);

            logger.LogInformation("Reservation {ReservationId} expired", reservation.Id);
            hasChanges = true;
        }

        if (hasChanges)
            await context.SaveChangesAsync(cancellationToken);
    }
}

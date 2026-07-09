using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reservations.Common;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Commands.CancelReservation;

public sealed class CancelReservationHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<CancelReservationHandler> logger) : IRequestHandler<CancelReservationCommand, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .Include(r => r.Car).ThenInclude(c => c.Brand)
            .Include(r => r.Car).ThenInclude(c => c.Model)
            .Include(r => r.User)
            .Include(r => r.PickupLocation)
            .Include(r => r.ReturnLocation)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        var isOwner = reservation.UserId == currentUserService.UserId;
        var isStaff = currentUserService.IsInRole("Admin") || currentUserService.IsInRole("Manager");

        if (!isOwner && !isStaff)
            throw new ForbiddenException("You can only cancel your own reservations.");

        if (reservation.Status is ReservationStatus.Completed)
            throw new BusinessRuleException("Completed reservations cannot be cancelled.");

        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new BusinessRuleException("This reservation is already cancelled or expired.");

        if (reservation.Status is ReservationStatus.Active)
            throw new BusinessRuleException("An active rental cannot be cancelled; complete it instead.");

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancellationReason = request.Reason;
        reservation.CancelledAt = DateTime.UtcNow;

        await CarStatusReconciler.ReconcileAsync(context, reservation.CarId, reservation.Id, cancellationToken);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Reservation),
            EntityId = reservation.Id.ToString(),
            Action = AuditAction.Update,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Reservation {ReservationId} cancelled", reservation.Id);

        return new ReservationResponse(
            reservation.Id, reservation.StartDate, reservation.EndDate, reservation.TotalPrice, reservation.Status,
            reservation.Notes, reservation.CancellationReason, reservation.CancelledAt,
            reservation.UserId, $"{reservation.User.FirstName} {reservation.User.LastName}", reservation.User.Email,
            reservation.CarId, reservation.Car.LicensePlate, reservation.Car.Brand.Name, reservation.Car.Model.Name,
            reservation.PickupLocationId, reservation.PickupLocation.Name,
            reservation.ReturnLocationId, reservation.ReturnLocation.Name,
            reservation.CreatedAt);
    }
}

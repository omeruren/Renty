using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Commands.ConfirmReservation;

public sealed class ConfirmReservationHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<ConfirmReservationHandler> logger) : IRequestHandler<ConfirmReservationCommand, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .Include(r => r.Car).ThenInclude(c => c.Brand)
            .Include(r => r.Car).ThenInclude(c => c.Model)
            .Include(r => r.User)
            .Include(r => r.PickupLocation)
            .Include(r => r.ReturnLocation)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        if (reservation.Status != ReservationStatus.Pending)
            throw new BusinessRuleException("Only pending reservations can be confirmed.");

        reservation.Status = ReservationStatus.Confirmed;

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

        logger.LogInformation("Reservation {ReservationId} confirmed", reservation.Id);

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

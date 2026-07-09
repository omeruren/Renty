using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Constants;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Commands.UpdateReservation;

public sealed class UpdateReservationHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<UpdateReservationHandler> logger) : IRequestHandler<UpdateReservationCommand, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .Include(r => r.Car).ThenInclude(c => c.Brand)
            .Include(r => r.Car).ThenInclude(c => c.Model)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        if (reservation.UserId != currentUserService.UserId && !currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("You can only modify your own reservations.");

        if (reservation.Status != ReservationStatus.Pending)
            throw new BusinessRuleException("Only pending reservations can be modified.");

        var pickupLocation = await context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.PickupLocationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.PickupLocationId);

        if (!pickupLocation.IsActive)
            throw new BusinessRuleException("The selected pickup location is not active.");

        var returnLocation = request.ReturnLocationId == request.PickupLocationId
            ? pickupLocation
            : await context.Locations
                .FirstOrDefaultAsync(l => l.Id == request.ReturnLocationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Location), request.ReturnLocationId);

        if (!returnLocation.IsActive)
            throw new BusinessRuleException("The selected return location is not active.");

        var hasOverlap = await context.Reservations.AnyAsync(
            r => r.Id != reservation.Id
                && r.CarId == reservation.CarId
                && ReservationStatuses.Blocking.Contains(r.Status)
                && r.StartDate < request.EndDate
                && request.StartDate < r.EndDate,
            cancellationToken);

        if (hasOverlap)
            throw new BusinessRuleException("The selected car is not available for the requested dates.");

        var days = (int)Math.Ceiling((request.EndDate - request.StartDate).TotalDays);

        reservation.StartDate = request.StartDate;
        reservation.EndDate = request.EndDate;
        reservation.PickupLocationId = request.PickupLocationId;
        reservation.ReturnLocationId = request.ReturnLocationId;
        reservation.Notes = request.Notes;
        reservation.TotalPrice = reservation.Car.DailyPrice * days;

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

        logger.LogInformation("Reservation {ReservationId} updated", reservation.Id);

        return new ReservationResponse(
            reservation.Id, reservation.StartDate, reservation.EndDate, reservation.TotalPrice, reservation.Status,
            reservation.Notes, reservation.CancellationReason, reservation.CancelledAt,
            reservation.UserId, $"{reservation.User.FirstName} {reservation.User.LastName}", reservation.User.Email,
            reservation.CarId, reservation.Car.LicensePlate, reservation.Car.Brand.Name, reservation.Car.Model.Name,
            reservation.PickupLocationId, pickupLocation.Name,
            reservation.ReturnLocationId, returnLocation.Name,
            reservation.CreatedAt);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Constants;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reservations.Common;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Commands.CreateReservation;

public sealed class CreateReservationHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<CreateReservationHandler> logger) : IRequestHandler<CreateReservationCommand, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("A signed-in user is required to create a reservation.");

        var car = await context.Cars
            .Include(c => c.Brand)
            .Include(c => c.Model)
            .FirstOrDefaultAsync(c => c.Id == request.CarId, cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.CarId);

        if (car.Status != CarStatus.Available)
            throw new BusinessRuleException("The selected car is not available for reservation.");

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
            r => r.CarId == request.CarId
                && ReservationStatuses.Blocking.Contains(r.Status)
                && r.StartDate < request.EndDate
                && request.StartDate < r.EndDate,
            cancellationToken);

        if (hasOverlap)
            throw new BusinessRuleException("The selected car is not available for the requested dates.");

        var days = (int)Math.Ceiling((request.EndDate - request.StartDate).TotalDays);

        var activeRules = await context.PricingRules
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var totalPrice = PricingCalculator.CalculateTotalPrice(
            car.DailyPrice, days, request.StartDate, car.Model.Category, activeRules);

        var reservation = new Reservation
        {
            CarId = request.CarId,
            UserId = userId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PickupLocationId = request.PickupLocationId,
            ReturnLocationId = request.ReturnLocationId,
            Notes = request.Notes,
            Status = ReservationStatus.Pending,
            TotalPrice = totalPrice
        };

        context.Reservations.Add(reservation);

        car.Status = CarStatus.Reserved;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Reservation),
            EntityId = reservation.Id.ToString(),
            Action = AuditAction.Create,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Reservation {ReservationId} created for car {CarId}", reservation.Id, car.Id);

        var user = await context.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        return new ReservationResponse(
            reservation.Id, reservation.StartDate, reservation.EndDate, reservation.TotalPrice, reservation.Status,
            reservation.Notes, reservation.CancellationReason, reservation.CancelledAt,
            reservation.UserId, $"{user.FirstName} {user.LastName}", user.Email,
            reservation.CarId, car.LicensePlate, car.Brand.Name, car.Model.Name,
            reservation.PickupLocationId, pickupLocation.Name,
            reservation.ReturnLocationId, returnLocation.Name,
            reservation.CreatedAt);
    }
}

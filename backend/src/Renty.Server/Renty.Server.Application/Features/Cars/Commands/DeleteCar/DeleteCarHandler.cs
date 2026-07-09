using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Cars.Commands.DeleteCar;

public sealed class DeleteCarHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<DeleteCarHandler> logger) : IRequestHandler<DeleteCarCommand>
{
    private static readonly ReservationStatus[] ActiveReservationStatuses =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed,
        ReservationStatus.Active
    ];

    public async Task Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        var car = await context.Cars
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.Id);

        var hasActiveReservations = await context.Reservations
            .AnyAsync(
                r => r.CarId == request.Id && ActiveReservationStatuses.Contains(r.Status),
                cancellationToken);

        if (hasActiveReservations)
            throw new BusinessRuleException("Cannot delete a car with active reservations.");

        context.Cars.Remove(car);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Car),
            EntityId = car.Id.ToString(),
            Action = AuditAction.Delete,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Car {CarId} deleted", car.Id);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Locations.Commands.DeleteLocation;

public sealed class DeleteLocationHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<DeleteLocationHandler> logger) : IRequestHandler<DeleteLocationCommand>
{
    public async Task Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.Id);

        var hasCars = await context.Cars.AnyAsync(c => c.LocationId == request.Id, cancellationToken);

        if (hasCars)
            throw new BusinessRuleException("Cannot delete a location with assigned cars.");

        var hasReservations = await context.Reservations
            .AnyAsync(r => r.PickupLocationId == request.Id || r.ReturnLocationId == request.Id, cancellationToken);

        if (hasReservations)
            throw new BusinessRuleException("Cannot delete a location referenced by a reservation.");

        context.Locations.Remove(location);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Location),
            EntityId = location.Id.ToString(),
            Action = AuditAction.Delete,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Location {LocationId} deleted", location.Id);
    }
}

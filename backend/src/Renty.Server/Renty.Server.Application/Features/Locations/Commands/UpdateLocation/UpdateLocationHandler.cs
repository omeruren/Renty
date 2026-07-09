using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Locations.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Locations.Commands.UpdateLocation;

public sealed class UpdateLocationHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<UpdateLocationHandler> logger) : IRequestHandler<UpdateLocationCommand, LocationResponse>
{
    public async Task<LocationResponse> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.Id);

        if (location.IsActive && !request.IsActive)
        {
            var hasAssignedCars = await context.Cars
                .AnyAsync(c => c.LocationId == request.Id, cancellationToken);

            if (hasAssignedCars)
                throw new BusinessRuleException("Cannot deactivate a location with assigned cars.");
        }

        location.Name = request.Name;
        location.Address = request.Address;
        location.City = request.City;
        location.District = request.District;
        location.PhoneNumber = request.PhoneNumber;
        location.Email = request.Email;
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.IsActive = request.IsActive;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Location),
            EntityId = location.Id.ToString(),
            Action = AuditAction.Update,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Location {LocationId} updated", location.Id);

        return new LocationResponse(
            location.Id, location.Name, location.Address, location.City, location.District,
            location.PhoneNumber, location.Email, location.Latitude, location.Longitude,
            location.IsActive, location.CreatedAt);
    }
}

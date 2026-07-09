using MediatR;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Locations.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Locations.Commands.CreateLocation;

public sealed class CreateLocationHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<CreateLocationHandler> logger) : IRequestHandler<CreateLocationCommand, LocationResponse>
{
    public async Task<LocationResponse> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = new Location
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            District = request.District,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsActive = true
        };

        context.Locations.Add(location);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Location),
            EntityId = location.Id.ToString(),
            Action = AuditAction.Create,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Location {LocationId} created", location.Id);

        return new LocationResponse(
            location.Id, location.Name, location.Address, location.City, location.District,
            location.PhoneNumber, location.Email, location.Latitude, location.Longitude,
            location.IsActive, location.CreatedAt);
    }
}

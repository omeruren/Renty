using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Cars.Commands.UpdateCar;

public sealed class UpdateCarHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<UpdateCarHandler> logger) : IRequestHandler<UpdateCarCommand, CarResponse>
{
    public async Task<CarResponse> Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        var car = await context.Cars
            .Include(c => c.Brand)
            .Include(c => c.Model)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.Id);

        var location = await context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        car.Color = request.Color;
        car.Mileage = request.Mileage;
        car.DailyPrice = request.DailyPrice;
        car.Status = request.Status;
        car.Description = request.Description;
        car.ImageUrl = request.ImageUrl;
        car.LocationId = request.LocationId;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Car),
            EntityId = car.Id.ToString(),
            Action = AuditAction.Update,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Car {CarId} updated", car.Id);

        return new CarResponse(
            car.Id, car.LicensePlate, car.Year, car.Color, car.Mileage, car.DailyPrice, car.Status,
            car.Description, car.ImageUrl,
            car.BrandId, car.Brand.Name,
            car.ModelId, car.Model.Name,
            car.LocationId, location.Name,
            car.CreatedAt);
    }
}

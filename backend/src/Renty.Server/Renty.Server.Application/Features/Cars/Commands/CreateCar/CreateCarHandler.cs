using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Cars.Commands.CreateCar;

public sealed class CreateCarHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<CreateCarHandler> logger) : IRequestHandler<CreateCarCommand, CarResponse>
{
    public async Task<CarResponse> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        var brand = await context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.BrandId, cancellationToken)
            ?? throw new NotFoundException(nameof(Brand), request.BrandId);

        var model = await context.Models
            .FirstOrDefaultAsync(m => m.Id == request.ModelId, cancellationToken)
            ?? throw new NotFoundException(nameof(Model), request.ModelId);

        if (model.BrandId != request.BrandId)
            throw new BusinessRuleException("The selected model does not belong to the selected brand.");

        var location = await context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        var plateExists = await context.Cars
            .AnyAsync(c => c.LicensePlate == request.LicensePlate, cancellationToken);

        if (plateExists)
            throw new BusinessRuleException("A car with this license plate already exists.");

        var car = new Car
        {
            LicensePlate = request.LicensePlate,
            Year = request.Year,
            Color = request.Color,
            Mileage = request.Mileage,
            DailyPrice = request.DailyPrice,
            Status = CarStatus.Available,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            BrandId = request.BrandId,
            ModelId = request.ModelId,
            LocationId = request.LocationId
        };

        context.Cars.Add(car);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Car),
            EntityId = car.Id.ToString(),
            Action = AuditAction.Create,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Car {CarId} created", car.Id);

        return new CarResponse(
            car.Id, car.LicensePlate, car.Year, car.Color, car.Mileage, car.DailyPrice, car.Status,
            car.Description, car.ImageUrl,
            car.BrandId, brand.Name,
            car.ModelId, model.Name,
            car.LocationId, location.Name,
            car.CreatedAt);
    }
}

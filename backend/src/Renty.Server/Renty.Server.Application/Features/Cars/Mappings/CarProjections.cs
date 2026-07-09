using System.Linq.Expressions;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Cars.Mappings;

public static class CarProjections
{
    public static Expression<Func<Car, CarResponse>> ToResponse { get; } =
        c => new CarResponse(
            c.Id, c.LicensePlate, c.Year, c.Color, c.Mileage, c.DailyPrice, c.Status,
            c.Description, c.ImageUrl,
            c.BrandId, c.Brand.Name,
            c.ModelId, c.Model.Name,
            c.LocationId, c.Location.Name,
            c.CreatedAt);

    public static Expression<Func<Car, CarListResponse>> ToListResponse { get; } =
        c => new CarListResponse(
            c.Id, c.LicensePlate, c.Year, c.Color, c.DailyPrice, c.Status,
            c.Brand.Name, c.Model.Name, c.Location.Name);
}

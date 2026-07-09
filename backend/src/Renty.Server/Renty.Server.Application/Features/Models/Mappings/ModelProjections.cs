using System.Linq.Expressions;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Models.Mappings;

public static class ModelProjections
{
    public static Expression<Func<Model, ModelResponse>> ToResponse { get; } =
        m => new ModelResponse(
            m.Id, m.Name, m.Category, m.SeatCount, m.TransmissionType, m.FuelType,
            m.BrandId, m.Brand.Name, m.CreatedAt);

    public static Expression<Func<Model, ModelListResponse>> ToListResponse { get; } =
        m => new ModelListResponse(
            m.Id, m.Name, m.Category, m.SeatCount, m.TransmissionType, m.FuelType,
            m.BrandId, m.Brand.Name);
}

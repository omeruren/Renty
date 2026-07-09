using System.Linq.Expressions;
using Renty.Server.Application.Features.Brands.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Brands.Mappings;

public static class BrandProjections
{
    public static Expression<Func<Brand, BrandResponse>> ToResponse { get; } =
        b => new BrandResponse(b.Id, b.Name, b.LogoUrl, b.CreatedAt);

    public static Expression<Func<Brand, BrandListResponse>> ToListResponse { get; } =
        b => new BrandListResponse(b.Id, b.Name, b.LogoUrl);
}

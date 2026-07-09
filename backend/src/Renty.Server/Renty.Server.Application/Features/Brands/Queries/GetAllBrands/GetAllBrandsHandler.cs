using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Brands.DTOs;
using Renty.Server.Application.Features.Brands.Mappings;

namespace Renty.Server.Application.Features.Brands.Queries.GetAllBrands;

public sealed class GetAllBrandsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllBrandsQuery, PagedResponse<BrandListResponse>>
{
    public async Task<PagedResponse<BrandListResponse>> Handle(
        GetAllBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Brands.AsNoTracking().OrderBy(b => b.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(BrandProjections.ToListResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<BrandListResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}

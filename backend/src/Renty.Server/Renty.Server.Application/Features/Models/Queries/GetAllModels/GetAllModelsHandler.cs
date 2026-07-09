using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Application.Features.Models.Mappings;

namespace Renty.Server.Application.Features.Models.Queries.GetAllModels;

public sealed class GetAllModelsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllModelsQuery, PagedResponse<ModelListResponse>>
{
    public async Task<PagedResponse<ModelListResponse>> Handle(
        GetAllModelsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Models.AsNoTracking().AsQueryable();

        if (request.BrandId.HasValue)
            query = query.Where(m => m.BrandId == request.BrandId.Value);

        query = query.OrderBy(m => m.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ModelProjections.ToListResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<ModelListResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}

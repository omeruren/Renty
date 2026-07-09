using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Application.Features.PricingRules.Mappings;

namespace Renty.Server.Application.Features.PricingRules.Queries.GetAllPricingRules;

public sealed class GetAllPricingRulesHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllPricingRulesQuery, PagedResponse<PricingRuleListResponse>>
{
    public async Task<PagedResponse<PricingRuleListResponse>> Handle(
        GetAllPricingRulesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.PricingRules.AsNoTracking().AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        query = query.OrderByDescending(p => p.Priority).ThenBy(p => p.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(PricingRuleProjections.ToListResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<PricingRuleListResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}

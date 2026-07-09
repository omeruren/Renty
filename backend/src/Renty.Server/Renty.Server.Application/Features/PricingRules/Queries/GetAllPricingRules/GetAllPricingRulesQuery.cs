using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.PricingRules.DTOs;

namespace Renty.Server.Application.Features.PricingRules.Queries.GetAllPricingRules;

public sealed record GetAllPricingRulesQuery(
    int Page = 1,
    int PageSize = 10,
    bool? IsActive = null) : IRequest<PagedResponse<PricingRuleListResponse>>;

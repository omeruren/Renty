using System.Linq.Expressions;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.PricingRules.Mappings;

public static class PricingRuleProjections
{
    public static Expression<Func<PricingRule, PricingRuleResponse>> ToResponse { get; } =
        p => new PricingRuleResponse(
            p.Id, p.Name, p.RuleType, p.Multiplier, p.StartDate, p.EndDate,
            p.VehicleCategory, p.IsActive, p.Priority, p.CreatedAt);

    public static Expression<Func<PricingRule, PricingRuleListResponse>> ToListResponse { get; } =
        p => new PricingRuleListResponse(p.Id, p.Name, p.RuleType, p.Multiplier, p.IsActive, p.Priority);
}

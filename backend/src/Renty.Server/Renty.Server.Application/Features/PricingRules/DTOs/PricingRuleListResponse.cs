using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.PricingRules.DTOs;

public sealed record PricingRuleListResponse(
    Guid Id,
    string Name,
    PricingRuleType RuleType,
    decimal Multiplier,
    bool IsActive,
    int Priority);

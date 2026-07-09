using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.PricingRules.DTOs;

public sealed record PricingRuleResponse(
    Guid Id,
    string Name,
    PricingRuleType RuleType,
    decimal Multiplier,
    DateTime? StartDate,
    DateTime? EndDate,
    VehicleCategory? VehicleCategory,
    bool IsActive,
    int Priority,
    DateTime CreatedAt);

using MediatR;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.PricingRules.Commands.UpdatePricingRule;

public sealed record UpdatePricingRuleCommand(
    Guid Id,
    string Name,
    PricingRuleType RuleType,
    decimal Multiplier,
    DateTime? StartDate,
    DateTime? EndDate,
    VehicleCategory? VehicleCategory,
    bool IsActive,
    int Priority) : IRequest<PricingRuleResponse>;

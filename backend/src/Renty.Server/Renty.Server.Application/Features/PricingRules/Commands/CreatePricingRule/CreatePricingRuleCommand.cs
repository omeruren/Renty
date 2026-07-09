using MediatR;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.PricingRules.Commands.CreatePricingRule;

public sealed record CreatePricingRuleCommand(
    string Name,
    PricingRuleType RuleType,
    decimal Multiplier,
    DateTime? StartDate,
    DateTime? EndDate,
    VehicleCategory? VehicleCategory,
    int Priority) : IRequest<PricingRuleResponse>;

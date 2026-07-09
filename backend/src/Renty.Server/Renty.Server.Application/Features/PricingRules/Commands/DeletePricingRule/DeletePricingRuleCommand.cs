using MediatR;

namespace Renty.Server.Application.Features.PricingRules.Commands.DeletePricingRule;

public sealed record DeletePricingRuleCommand(Guid Id) : IRequest;

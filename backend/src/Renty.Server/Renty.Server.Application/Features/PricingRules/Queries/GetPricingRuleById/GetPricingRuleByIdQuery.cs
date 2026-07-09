using MediatR;
using Renty.Server.Application.Features.PricingRules.DTOs;

namespace Renty.Server.Application.Features.PricingRules.Queries.GetPricingRuleById;

public sealed record GetPricingRuleByIdQuery(Guid Id) : IRequest<PricingRuleResponse>;

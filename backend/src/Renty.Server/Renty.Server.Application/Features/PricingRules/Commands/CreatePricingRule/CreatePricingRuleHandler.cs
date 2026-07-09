using MediatR;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.PricingRules.Commands.CreatePricingRule;

public sealed class CreatePricingRuleHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<CreatePricingRuleHandler> logger) : IRequestHandler<CreatePricingRuleCommand, PricingRuleResponse>
{
    public async Task<PricingRuleResponse> Handle(CreatePricingRuleCommand request, CancellationToken cancellationToken)
    {
        var pricingRule = new PricingRule
        {
            Name = request.Name,
            RuleType = request.RuleType,
            Multiplier = request.Multiplier,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            VehicleCategory = request.VehicleCategory,
            Priority = request.Priority,
            IsActive = true
        };

        context.PricingRules.Add(pricingRule);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(PricingRule),
            EntityId = pricingRule.Id.ToString(),
            Action = AuditAction.Create,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pricing rule {PricingRuleId} created", pricingRule.Id);

        return new PricingRuleResponse(
            pricingRule.Id, pricingRule.Name, pricingRule.RuleType, pricingRule.Multiplier,
            pricingRule.StartDate, pricingRule.EndDate, pricingRule.VehicleCategory,
            pricingRule.IsActive, pricingRule.Priority, pricingRule.CreatedAt);
    }
}

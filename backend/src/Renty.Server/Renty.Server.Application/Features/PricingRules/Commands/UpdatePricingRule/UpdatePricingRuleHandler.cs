using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.PricingRules.Commands.UpdatePricingRule;

public sealed class UpdatePricingRuleHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<UpdatePricingRuleHandler> logger) : IRequestHandler<UpdatePricingRuleCommand, PricingRuleResponse>
{
    public async Task<PricingRuleResponse> Handle(UpdatePricingRuleCommand request, CancellationToken cancellationToken)
    {
        var pricingRule = await context.PricingRules
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PricingRule), request.Id);

        pricingRule.Name = request.Name;
        pricingRule.RuleType = request.RuleType;
        pricingRule.Multiplier = request.Multiplier;
        pricingRule.StartDate = request.StartDate;
        pricingRule.EndDate = request.EndDate;
        pricingRule.VehicleCategory = request.VehicleCategory;
        pricingRule.IsActive = request.IsActive;
        pricingRule.Priority = request.Priority;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(PricingRule),
            EntityId = pricingRule.Id.ToString(),
            Action = AuditAction.Update,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pricing rule {PricingRuleId} updated", pricingRule.Id);

        return new PricingRuleResponse(
            pricingRule.Id, pricingRule.Name, pricingRule.RuleType, pricingRule.Multiplier,
            pricingRule.StartDate, pricingRule.EndDate, pricingRule.VehicleCategory,
            pricingRule.IsActive, pricingRule.Priority, pricingRule.CreatedAt);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.PricingRules.Commands.DeletePricingRule;

public sealed class DeletePricingRuleHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<DeletePricingRuleHandler> logger) : IRequestHandler<DeletePricingRuleCommand>
{
    public async Task Handle(DeletePricingRuleCommand request, CancellationToken cancellationToken)
    {
        var pricingRule = await context.PricingRules
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PricingRule), request.Id);

        context.PricingRules.Remove(pricingRule);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(PricingRule),
            EntityId = pricingRule.Id.ToString(),
            Action = AuditAction.Delete,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Pricing rule {PricingRuleId} deleted", pricingRule.Id);
    }
}

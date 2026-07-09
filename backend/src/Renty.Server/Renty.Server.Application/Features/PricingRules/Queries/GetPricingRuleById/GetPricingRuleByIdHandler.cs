using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Application.Features.PricingRules.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.PricingRules.Queries.GetPricingRuleById;

public sealed class GetPricingRuleByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetPricingRuleByIdQuery, PricingRuleResponse>
{
    public async Task<PricingRuleResponse> Handle(GetPricingRuleByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.PricingRules
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(PricingRuleProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(PricingRule), request.Id);
    }
}

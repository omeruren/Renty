using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Brands.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<UpdateBrandHandler> logger) : IRequestHandler<UpdateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Brand), request.Id);

        var nameTaken = await context.Brands
            .AnyAsync(b => b.Name == request.Name && b.Id != request.Id, cancellationToken);

        if (nameTaken)
            throw new BusinessRuleException("A brand with this name already exists.");

        brand.Name = request.Name;
        brand.LogoUrl = request.LogoUrl;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Brand),
            EntityId = brand.Id.ToString(),
            Action = AuditAction.Update,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Brand {BrandId} updated", brand.Id);

        return new BrandResponse(brand.Id, brand.Name, brand.LogoUrl, brand.CreatedAt);
    }
}

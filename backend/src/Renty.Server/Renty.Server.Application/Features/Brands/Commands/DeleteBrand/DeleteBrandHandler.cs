using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Brands.Commands.DeleteBrand;

public sealed class DeleteBrandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<DeleteBrandHandler> logger) : IRequestHandler<DeleteBrandCommand>
{
    public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Brand), request.Id);

        var hasModels = await context.Models.AnyAsync(m => m.BrandId == request.Id, cancellationToken);

        if (hasModels)
            throw new BusinessRuleException("Cannot delete a brand with existing models.");

        var hasCars = await context.Cars.AnyAsync(c => c.BrandId == request.Id, cancellationToken);

        if (hasCars)
            throw new BusinessRuleException("Cannot delete a brand with existing cars.");

        context.Brands.Remove(brand);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Brand),
            EntityId = brand.Id.ToString(),
            Action = AuditAction.Delete,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Brand {BrandId} deleted", brand.Id);
    }
}

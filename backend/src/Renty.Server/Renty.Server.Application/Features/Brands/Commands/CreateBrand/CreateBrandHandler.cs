using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Brands.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<CreateBrandHandler> logger) : IRequestHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await context.Brands.AnyAsync(b => b.Name == request.Name, cancellationToken);

        if (nameExists)
            throw new BusinessRuleException("A brand with this name already exists.");

        var brand = new Brand
        {
            Name = request.Name,
            LogoUrl = request.LogoUrl
        };

        context.Brands.Add(brand);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Brand),
            EntityId = brand.Id.ToString(),
            Action = AuditAction.Create,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Brand {BrandId} created", brand.Id);

        return new BrandResponse(brand.Id, brand.Name, brand.LogoUrl, brand.CreatedAt);
    }
}

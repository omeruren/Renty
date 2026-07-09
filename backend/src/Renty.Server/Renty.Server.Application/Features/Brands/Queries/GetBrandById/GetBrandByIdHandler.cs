using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Brands.DTOs;
using Renty.Server.Application.Features.Brands.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetBrandByIdQuery, BrandResponse>
{
    public async Task<BrandResponse> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Brands
            .AsNoTracking()
            .Where(b => b.Id == request.Id)
            .Select(BrandProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Brand), request.Id);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Locations.DTOs;
using Renty.Server.Application.Features.Locations.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Locations.Queries.GetLocationById;

public sealed class GetLocationByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetLocationByIdQuery, LocationResponse>
{
    public async Task<LocationResponse> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Locations
            .AsNoTracking()
            .Where(l => l.Id == request.Id)
            .Select(LocationProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Location), request.Id);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Application.Features.Cars.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Cars.Queries.GetCarById;

public sealed class GetCarByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetCarByIdQuery, CarResponse>
{
    public async Task<CarResponse> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Cars
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(CarProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Car), request.Id);
    }
}

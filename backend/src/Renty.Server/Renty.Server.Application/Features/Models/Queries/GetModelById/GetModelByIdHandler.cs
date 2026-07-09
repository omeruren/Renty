using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Application.Features.Models.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Models.Queries.GetModelById;

public sealed class GetModelByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetModelByIdQuery, ModelResponse>
{
    public async Task<ModelResponse> Handle(GetModelByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Models
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(ModelProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Model), request.Id);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Users.DTOs;
using Renty.Server.Application.Features.Users.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .Select(UserProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Users.DTOs;
using Renty.Server.Application.Features.Users.Mappings;

namespace Renty.Server.Application.Features.Users.Queries.GetAllUsers;

public sealed class GetAllUsersHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllUsersQuery, PagedResponse<UserListResponse>>
{
    public async Task<PagedResponse<UserListResponse>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Users.AsNoTracking().OrderBy(u => u.Email);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(UserProjections.ToListResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserListResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}

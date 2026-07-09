using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Users.DTOs;

namespace Renty.Server.Application.Features.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(int Page = 1, int PageSize = 10)
    : IRequest<PagedResponse<UserListResponse>>;

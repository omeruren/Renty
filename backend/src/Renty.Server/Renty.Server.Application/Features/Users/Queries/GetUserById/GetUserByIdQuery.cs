using MediatR;
using Renty.Server.Application.Features.Users.DTOs;

namespace Renty.Server.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserResponse>;

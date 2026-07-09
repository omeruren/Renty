using MediatR;
using Renty.Server.Application.Features.Models.DTOs;

namespace Renty.Server.Application.Features.Models.Queries.GetModelById;

public sealed record GetModelByIdQuery(Guid Id) : IRequest<ModelResponse>;

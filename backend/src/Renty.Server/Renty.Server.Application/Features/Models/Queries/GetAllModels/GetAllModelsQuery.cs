using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Models.DTOs;

namespace Renty.Server.Application.Features.Models.Queries.GetAllModels;

public sealed record GetAllModelsQuery(int Page = 1, int PageSize = 10, Guid? BrandId = null)
    : IRequest<PagedResponse<ModelListResponse>>;

using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Brands.DTOs;

namespace Renty.Server.Application.Features.Brands.Queries.GetAllBrands;

public sealed record GetAllBrandsQuery(int Page = 1, int PageSize = 10)
    : IRequest<PagedResponse<BrandListResponse>>;

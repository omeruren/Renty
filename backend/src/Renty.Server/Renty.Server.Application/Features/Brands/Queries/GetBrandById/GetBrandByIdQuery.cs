using MediatR;
using Renty.Server.Application.Features.Brands.DTOs;

namespace Renty.Server.Application.Features.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(Guid Id) : IRequest<BrandResponse>;

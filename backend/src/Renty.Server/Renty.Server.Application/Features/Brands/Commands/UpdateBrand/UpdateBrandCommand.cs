using MediatR;
using Renty.Server.Application.Features.Brands.DTOs;

namespace Renty.Server.Application.Features.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand(Guid Id, string Name, string? LogoUrl) : IRequest<BrandResponse>;

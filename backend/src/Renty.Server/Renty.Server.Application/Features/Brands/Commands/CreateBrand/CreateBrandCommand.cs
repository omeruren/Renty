using MediatR;
using Renty.Server.Application.Features.Brands.DTOs;

namespace Renty.Server.Application.Features.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(string Name, string? LogoUrl) : IRequest<BrandResponse>;

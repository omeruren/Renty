namespace Renty.Server.Application.Features.Brands.DTOs;

public sealed record BrandResponse(Guid Id, string Name, string? LogoUrl, DateTime CreatedAt);

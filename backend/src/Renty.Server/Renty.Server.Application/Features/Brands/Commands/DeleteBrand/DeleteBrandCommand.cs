using MediatR;

namespace Renty.Server.Application.Features.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand(Guid Id) : IRequest;

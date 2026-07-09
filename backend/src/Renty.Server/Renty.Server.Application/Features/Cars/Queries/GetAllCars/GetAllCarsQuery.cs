using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Cars.Queries.GetAllCars;

public sealed record GetAllCarsQuery(
    int Page = 1,
    int PageSize = 10,
    CarStatus? Status = null,
    Guid? BrandId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string SortBy = "licensePlate",
    string SortOrder = "asc") : IRequest<PagedResponse<CarListResponse>>;

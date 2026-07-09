using FluentValidation;
using Renty.Server.Application.Common.Models;

namespace Renty.Server.Application.Features.Cars.Queries.GetAllCars;

public sealed class GetAllCarsValidator : AbstractValidator<GetAllCarsQuery>
{
    private static readonly string[] AllowedSortFields = ["licenseplate", "year", "dailyprice"];
    private static readonly string[] AllowedSortOrders = ["asc", "desc"];

    public GetAllCarsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {PaginationRequest.MaxPageSize}.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum price must not be negative.")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum price must not be negative.")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price must not exceed maximum price.")
            .WithName("MinPrice");

        RuleFor(x => x.SortBy)
            .Must(value => AllowedSortFields.Contains(value.ToLowerInvariant()))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");

        RuleFor(x => x.SortOrder)
            .Must(value => AllowedSortOrders.Contains(value.ToLowerInvariant()))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}

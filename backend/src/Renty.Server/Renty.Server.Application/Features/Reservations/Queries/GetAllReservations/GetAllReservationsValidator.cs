using FluentValidation;
using Renty.Server.Application.Common.Models;

namespace Renty.Server.Application.Features.Reservations.Queries.GetAllReservations;

public sealed class GetAllReservationsValidator : AbstractValidator<GetAllReservationsQuery>
{
    private static readonly string[] AllowedSortFields = ["startdate", "enddate", "totalprice", "createdat"];
    private static readonly string[] AllowedSortOrders = ["asc", "desc"];

    public GetAllReservationsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {PaginationRequest.MaxPageSize}.");

        RuleFor(x => x.SortBy)
            .Must(value => AllowedSortFields.Contains(value.ToLowerInvariant()))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");

        RuleFor(x => x.SortOrder)
            .Must(value => AllowedSortOrders.Contains(value.ToLowerInvariant()))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}

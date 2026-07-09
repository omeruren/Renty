using FluentValidation;
using Microsoft.Extensions.Options;
using Renty.Server.Application.Common.Configuration;

namespace Renty.Server.Application.Features.Reservations.Commands.UpdateReservation;

public sealed class UpdateReservationValidator : AbstractValidator<UpdateReservationCommand>
{
    public UpdateReservationValidator(IOptions<ReservationSettings> reservationSettings)
    {
        var settings = reservationSettings.Value;

        RuleFor(x => x.PickupLocationId).NotEmpty();
        RuleFor(x => x.ReturnLocationId).NotEmpty();

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Start date must be in the future.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");

        RuleFor(x => x)
            .Must(x => Math.Ceiling((x.EndDate - x.StartDate).TotalDays) >= settings.MinDurationDays)
            .WithMessage($"Minimum rental duration is {settings.MinDurationDays} day(s).")
            .WithName("EndDate")
            .When(x => x.EndDate > x.StartDate);

        RuleFor(x => x)
            .Must(x => Math.Ceiling((x.EndDate - x.StartDate).TotalDays) <= settings.MaxDurationDays)
            .WithMessage($"Maximum rental duration is {settings.MaxDurationDays} day(s).")
            .WithName("EndDate")
            .When(x => x.EndDate > x.StartDate);

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}

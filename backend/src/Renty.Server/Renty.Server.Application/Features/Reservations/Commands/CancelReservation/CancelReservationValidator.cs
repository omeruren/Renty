using FluentValidation;

namespace Renty.Server.Application.Features.Reservations.Commands.CancelReservation;

public sealed class CancelReservationValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

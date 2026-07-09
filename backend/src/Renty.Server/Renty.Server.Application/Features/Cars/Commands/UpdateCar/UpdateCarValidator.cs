using FluentValidation;

namespace Renty.Server.Application.Features.Cars.Commands.UpdateCar;

public sealed class UpdateCarValidator : AbstractValidator<UpdateCarCommand>
{
    public UpdateCarValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required.")
            .MaximumLength(50).WithMessage("Color must not exceed 50 characters.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage must not be negative.");

        RuleFor(x => x.DailyPrice)
            .GreaterThan(0).WithMessage("Daily price must be greater than zero.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be valid.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.")
            .When(x => x.ImageUrl is not null);

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location is required.");
    }
}

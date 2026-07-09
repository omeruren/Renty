using FluentValidation;

namespace Renty.Server.Application.Features.Cars.Commands.CreateCar;

public sealed class CreateCarValidator : AbstractValidator<CreateCarCommand>
{
    public CreateCarValidator()
    {
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("License plate is required.")
            .MaximumLength(20).WithMessage("License plate must not exceed 20 characters.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required.")
            .MaximumLength(50).WithMessage("Color must not exceed 50 characters.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage must not be negative.");

        RuleFor(x => x.DailyPrice)
            .GreaterThan(0).WithMessage("Daily price must be greater than zero.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.")
            .When(x => x.ImageUrl is not null);

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand is required.");

        RuleFor(x => x.ModelId)
            .NotEmpty().WithMessage("Model is required.");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location is required.");
    }
}

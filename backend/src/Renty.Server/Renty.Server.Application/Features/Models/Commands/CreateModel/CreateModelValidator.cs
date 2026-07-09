using FluentValidation;

namespace Renty.Server.Application.Features.Models.Commands.CreateModel;

public sealed class CreateModelValidator : AbstractValidator<CreateModelCommand>
{
    public CreateModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid vehicle category.");

        RuleFor(x => x.SeatCount)
            .InclusiveBetween(1, 50).WithMessage("Seat count must be between 1 and 50.");

        RuleFor(x => x.TransmissionType)
            .IsInEnum().WithMessage("Transmission type must be valid.");

        RuleFor(x => x.FuelType)
            .IsInEnum().WithMessage("Fuel type must be valid.");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand is required.");
    }
}

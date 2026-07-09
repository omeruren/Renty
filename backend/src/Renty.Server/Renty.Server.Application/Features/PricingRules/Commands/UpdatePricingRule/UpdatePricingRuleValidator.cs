using FluentValidation;

namespace Renty.Server.Application.Features.PricingRules.Commands.UpdatePricingRule;

public sealed class UpdatePricingRuleValidator : AbstractValidator<UpdatePricingRuleCommand>
{
    public UpdatePricingRuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.RuleType)
            .IsInEnum().WithMessage("A valid rule type is required.");

        RuleFor(x => x.Multiplier)
            .GreaterThan(0).WithMessage("Multiplier must be greater than zero.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be greater than or equal to zero.");

        RuleFor(x => x.VehicleCategory)
            .IsInEnum().WithMessage("A valid vehicle category is required.")
            .When(x => x.VehicleCategory.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

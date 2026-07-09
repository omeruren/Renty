using FluentValidation;

namespace Renty.Server.Application.Features.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL must not exceed 500 characters.")
            .When(x => x.LogoUrl is not null);
    }
}

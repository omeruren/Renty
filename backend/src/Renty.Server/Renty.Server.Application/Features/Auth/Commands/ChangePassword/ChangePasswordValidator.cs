using FluentValidation;
using Renty.Server.Application.Common.Validators;

namespace Renty.Server.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword).MustBeStrongPassword();
    }
}

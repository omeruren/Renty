using FluentValidation;

namespace Renty.Server.Application.Features.Users.Commands.AssignRoles;

public sealed class AssignRolesValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesValidator()
    {
        RuleFor(x => x.RoleIds)
            .NotEmpty().WithMessage("At least one role must be assigned.");
    }
}

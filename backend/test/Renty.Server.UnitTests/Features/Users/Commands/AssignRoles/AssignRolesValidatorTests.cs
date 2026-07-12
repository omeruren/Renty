using FluentAssertions;
using Renty.Server.Application.Features.Users.Commands.AssignRoles;

namespace Renty.Server.UnitTests.Features.Users.Commands.AssignRoles;

public sealed class AssignRolesValidatorTests
{
    private readonly AssignRolesValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new AssignRolesCommand(Guid.NewGuid(), [Guid.NewGuid()]));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyRoleIds_ReturnsValidationError()
    {
        var result = _validator.Validate(new AssignRolesCommand(Guid.NewGuid(), []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignRolesCommand.RoleIds));
    }
}

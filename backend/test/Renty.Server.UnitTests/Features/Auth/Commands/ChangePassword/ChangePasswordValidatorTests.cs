using FluentAssertions;
using Renty.Server.Application.Features.Auth.Commands.ChangePassword;

namespace Renty.Server.UnitTests.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new ChangePasswordCommand("current", "New-Str0ng!Pass"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyCurrentPassword_ReturnsValidationError()
    {
        var result = _validator.Validate(new ChangePasswordCommand("", "New-Str0ng!Pass"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.CurrentPassword));
    }

    [Fact]
    public void Validate_WeakNewPassword_ReturnsValidationError()
    {
        var result = _validator.Validate(new ChangePasswordCommand("current", "weak"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }
}

using FluentAssertions;
using Renty.Server.Application.Features.Auth.Commands.Login;

namespace Renty.Server.UnitTests.Features.Auth.Commands.Login;

public sealed class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new LoginCommand("jane@example.com", "password"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsValidationError()
    {
        var result = _validator.Validate(new LoginCommand("not-an-email", "password"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_EmptyPassword_ReturnsValidationError()
    {
        var result = _validator.Validate(new LoginCommand("jane@example.com", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }
}

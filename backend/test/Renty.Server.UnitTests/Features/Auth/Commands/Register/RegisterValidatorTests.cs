using FluentAssertions;
using Renty.Server.Application.Features.Auth.Commands.Register;

namespace Renty.Server.UnitTests.Features.Auth.Commands.Register;

public sealed class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("jane@example.com", "Str0ng!Pass", "Jane", "Doe", null);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChar1")]
    public void Validate_WeakPassword_ReturnsValidationError(string password)
    {
        var command = ValidCommand() with { Password = password };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    [Theory]
    [InlineData("password")]
    [InlineData("welcome1")]
    [InlineData("PASSWORD")]
    public void Validate_CommonPassword_ReturnsValidationErrorMentioningCommon(string password)
    {
        var command = ValidCommand() with { Password = password };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(RegisterCommand.Password) && e.ErrorMessage.Contains("too common"));
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsValidationError()
    {
        var command = ValidCommand() with { Email = "not-an-email" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Fact]
    public void Validate_EmptyFirstName_ReturnsValidationError()
    {
        var command = ValidCommand() with { FirstName = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.FirstName));
    }
}

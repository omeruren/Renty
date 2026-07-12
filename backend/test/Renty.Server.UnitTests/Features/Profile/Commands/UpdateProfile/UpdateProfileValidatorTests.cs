using FluentAssertions;
using Renty.Server.Application.Features.Profile.Commands.UpdateProfile;

namespace Renty.Server.UnitTests.Features.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileValidatorTests
{
    private readonly UpdateProfileValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new UpdateProfileCommand("Jane", "Doe", "555-9999", null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyFirstName_ReturnsValidationError()
    {
        var result = _validator.Validate(new UpdateProfileCommand("", "Doe", null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfileCommand.FirstName));
    }

    [Fact]
    public void Validate_EmptyLastName_ReturnsValidationError()
    {
        var result = _validator.Validate(new UpdateProfileCommand("Jane", "", null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfileCommand.LastName));
    }
}

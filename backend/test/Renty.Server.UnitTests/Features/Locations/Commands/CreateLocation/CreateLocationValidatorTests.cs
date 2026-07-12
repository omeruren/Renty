using FluentAssertions;
using Renty.Server.Application.Features.Locations.Commands.CreateLocation;

namespace Renty.Server.UnitTests.Features.Locations.Commands.CreateLocation;

public sealed class CreateLocationValidatorTests
{
    private readonly CreateLocationValidator _validator = new();

    private static CreateLocationCommand ValidCommand() =>
        new("Downtown", "123 Main St", "Istanbul", "Kadikoy", "555-1234", "downtown@renty.dev", 40.99, 29.03);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyAddress_ReturnsValidationError()
    {
        var command = ValidCommand() with { Address = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateLocationCommand.Address));
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsValidationError()
    {
        var command = ValidCommand() with { Email = "not-an-email" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateLocationCommand.Email));
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_LatitudeOutOfRange_ReturnsValidationError(double latitude)
    {
        var command = ValidCommand() with { Latitude = latitude };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateLocationCommand.Latitude));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_LongitudeOutOfRange_ReturnsValidationError(double longitude)
    {
        var command = ValidCommand() with { Longitude = longitude };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateLocationCommand.Longitude));
    }
}

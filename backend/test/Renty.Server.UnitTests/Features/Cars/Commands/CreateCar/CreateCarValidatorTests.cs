using FluentAssertions;
using Renty.Server.Application.Features.Cars.Commands.CreateCar;

namespace Renty.Server.UnitTests.Features.Cars.Commands.CreateCar;

public sealed class CreateCarValidatorTests
{
    private readonly CreateCarValidator _validator = new();

    private static CreateCarCommand ValidCommand() => new(
        "34ABC123", 2024, "Black", 1500, 850m, "desc", null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyLicensePlate_ReturnsValidationError()
    {
        var command = ValidCommand() with { LicensePlate = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarCommand.LicensePlate));
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2100)]
    public void Validate_YearOutOfRange_ReturnsValidationError(int year)
    {
        var command = ValidCommand() with { Year = year };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarCommand.Year));
    }

    [Fact]
    public void Validate_NegativeMileage_ReturnsValidationError()
    {
        var command = ValidCommand() with { Mileage = -1 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarCommand.Mileage));
    }

    [Fact]
    public void Validate_ZeroDailyPrice_ReturnsValidationError()
    {
        var command = ValidCommand() with { DailyPrice = 0 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarCommand.DailyPrice));
    }

    [Fact]
    public void Validate_EmptyBrandId_ReturnsValidationError()
    {
        var command = ValidCommand() with { BrandId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarCommand.BrandId));
    }
}

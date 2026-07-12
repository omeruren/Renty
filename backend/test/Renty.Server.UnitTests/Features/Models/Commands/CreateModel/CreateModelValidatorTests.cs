using FluentAssertions;
using Renty.Server.Application.Features.Models.Commands.CreateModel;
using Renty.Server.Domain.Enums;

namespace Renty.Server.UnitTests.Features.Models.Commands.CreateModel;

public sealed class CreateModelValidatorTests
{
    private readonly CreateModelValidator _validator = new();

    private static CreateModelCommand ValidCommand() =>
        new("Corolla", VehicleCategory.Sedan, 5, TransmissionType.Automatic, FuelType.Gasoline, Guid.NewGuid());

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyName_ReturnsValidationError()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateModelCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_SeatCountOutOfRange_ReturnsValidationError(int seatCount)
    {
        var command = ValidCommand() with { SeatCount = seatCount };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateModelCommand.SeatCount));
    }

    [Fact]
    public void Validate_EmptyBrandId_ReturnsValidationError()
    {
        var command = ValidCommand() with { BrandId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateModelCommand.BrandId));
    }
}

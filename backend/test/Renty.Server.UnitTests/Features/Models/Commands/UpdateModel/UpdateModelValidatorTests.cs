using FluentAssertions;
using Renty.Server.Application.Features.Models.Commands.UpdateModel;
using Renty.Server.Domain.Enums;

namespace Renty.Server.UnitTests.Features.Models.Commands.UpdateModel;

public sealed class UpdateModelValidatorTests
{
    private readonly UpdateModelValidator _validator = new();

    private static UpdateModelCommand ValidCommand() =>
        new(Guid.NewGuid(), "Camry", VehicleCategory.Sedan, 5, TransmissionType.Automatic, FuelType.Gasoline);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyId_ReturnsValidationError()
    {
        var command = ValidCommand() with { Id = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateModelCommand.Id));
    }

    [Fact]
    public void Validate_EmptyName_ReturnsValidationError()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateModelCommand.Name));
    }
}

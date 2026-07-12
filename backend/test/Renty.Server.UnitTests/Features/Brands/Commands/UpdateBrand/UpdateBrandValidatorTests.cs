using FluentAssertions;
using Renty.Server.Application.Features.Brands.Commands.UpdateBrand;

namespace Renty.Server.UnitTests.Features.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandValidatorTests
{
    private readonly UpdateBrandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new UpdateBrandCommand(Guid.NewGuid(), "Toyota", null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyId_ReturnsValidationError()
    {
        var result = _validator.Validate(new UpdateBrandCommand(Guid.Empty, "Toyota", null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateBrandCommand.Id));
    }

    [Fact]
    public void Validate_EmptyName_ReturnsValidationError()
    {
        var result = _validator.Validate(new UpdateBrandCommand(Guid.NewGuid(), "", null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateBrandCommand.Name));
    }
}

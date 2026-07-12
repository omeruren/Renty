using FluentAssertions;
using Renty.Server.Application.Features.Brands.Commands.CreateBrand;

namespace Renty.Server.UnitTests.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandValidatorTests
{
    private readonly CreateBrandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new CreateBrandCommand("Toyota", null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyName_ReturnsValidationError()
    {
        var result = _validator.Validate(new CreateBrandCommand("", null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBrandCommand.Name));
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_ReturnsValidationError()
    {
        var result = _validator.Validate(new CreateBrandCommand(new string('a', 101), null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBrandCommand.Name));
    }
}

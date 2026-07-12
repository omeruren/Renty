using FluentAssertions;
using Renty.Server.Application.Features.Auth.Commands.Refresh;

namespace Renty.Server.UnitTests.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenValidatorTests
{
    private readonly RefreshTokenValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new RefreshTokenCommand("some-token"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyToken_ReturnsValidationError()
    {
        var result = _validator.Validate(new RefreshTokenCommand(""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken));
    }
}

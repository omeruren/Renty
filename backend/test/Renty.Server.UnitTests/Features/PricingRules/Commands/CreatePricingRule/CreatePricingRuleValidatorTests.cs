using FluentAssertions;
using Renty.Server.Application.Features.PricingRules.Commands.CreatePricingRule;
using Renty.Server.Domain.Enums;

namespace Renty.Server.UnitTests.Features.PricingRules.Commands.CreatePricingRule;

public sealed class CreatePricingRuleValidatorTests
{
    private readonly CreatePricingRuleValidator _validator = new();

    private static CreatePricingRuleCommand ValidCommand() =>
        new("Weekend Surcharge", PricingRuleType.WeekendMultiplier, 1.2m, null, null, null, 1);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ZeroMultiplier_ReturnsValidationError()
    {
        var command = ValidCommand() with { Multiplier = 0 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePricingRuleCommand.Multiplier));
    }

    [Fact]
    public void Validate_NegativePriority_ReturnsValidationError()
    {
        var command = ValidCommand() with { Priority = -1 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePricingRuleCommand.Priority));
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_ReturnsValidationError()
    {
        var start = DateTime.UtcNow;
        var command = ValidCommand() with { StartDate = start, EndDate = start.AddDays(-1) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePricingRuleCommand.EndDate));
    }
}

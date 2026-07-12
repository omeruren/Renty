using FluentAssertions;
using Microsoft.Extensions.Options;
using Renty.Server.Application.Common.Configuration;
using Renty.Server.Application.Features.Reservations.Commands.CreateReservation;

namespace Renty.Server.UnitTests.Features.Reservations.Commands.CreateReservation;

public sealed class CreateReservationValidatorTests
{
    private readonly CreateReservationValidator _validator =
        new(Options.Create(new ReservationSettings { MinDurationDays = 1, MaxDurationDays = 30 }));

    private static CreateReservationCommand ValidCommand() => new(
        Guid.NewGuid(),
        DateTime.UtcNow.AddDays(2),
        DateTime.UtcNow.AddDays(5),
        Guid.NewGuid(),
        Guid.NewGuid(),
        null);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_StartDateInPast_ReturnsValidationError()
    {
        var command = ValidCommand() with { StartDate = DateTime.UtcNow.AddDays(-1) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReservationCommand.StartDate));
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_ReturnsValidationError()
    {
        var start = DateTime.UtcNow.AddDays(5);
        var command = ValidCommand() with { StartDate = start, EndDate = start.AddDays(-1) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReservationCommand.EndDate));
    }

    [Fact]
    public void Validate_DurationBelowMinimum_ReturnsValidationError()
    {
        var validator = new CreateReservationValidator(Options.Create(new ReservationSettings { MinDurationDays = 2, MaxDurationDays = 30 }));
        var start = DateTime.UtcNow.AddDays(2);
        var command = ValidCommand() with { StartDate = start, EndDate = start.AddHours(12) };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
    }

    [Fact]
    public void Validate_DurationAboveMaximum_ReturnsValidationError()
    {
        var validator = new CreateReservationValidator(Options.Create(new ReservationSettings { MinDurationDays = 1, MaxDurationDays = 5 }));
        var start = DateTime.UtcNow.AddDays(2);
        var command = ValidCommand() with { StartDate = start, EndDate = start.AddDays(10) };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
    }

    [Fact]
    public void Validate_EmptyCarId_ReturnsValidationError()
    {
        var command = ValidCommand() with { CarId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReservationCommand.CarId));
    }
}

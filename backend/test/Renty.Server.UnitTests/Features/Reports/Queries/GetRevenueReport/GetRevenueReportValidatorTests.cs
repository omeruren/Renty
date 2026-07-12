using FluentAssertions;
using Renty.Server.Application.Features.Reports.Queries.GetRevenueReport;

namespace Renty.Server.UnitTests.Features.Reports.Queries.GetRevenueReport;

public sealed class GetRevenueReportValidatorTests
{
    private readonly GetRevenueReportValidator _validator = new();

    [Fact]
    public void Validate_NoDateRange_HasNoErrors()
    {
        var result = _validator.Validate(new GetRevenueReportQuery(null, null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ToBeforeFrom_ReturnsValidationError()
    {
        var from = new DateTime(2026, 3, 1);
        var result = _validator.Validate(new GetRevenueReportQuery(from, from.AddDays(-1)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetRevenueReportQuery.To));
    }

    [Fact]
    public void Validate_ToAfterFrom_HasNoErrors()
    {
        var from = new DateTime(2026, 3, 1);
        var result = _validator.Validate(new GetRevenueReportQuery(from, from.AddDays(1)));

        result.IsValid.Should().BeTrue();
    }
}

using FluentAssertions;
using Renty.Server.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

namespace Renty.Server.UnitTests.Features.AuditLogs.Queries.GetAllAuditLogs;

public sealed class GetAllAuditLogsValidatorTests
{
    private readonly GetAllAuditLogsValidator _validator = new();

    [Fact]
    public void Validate_DefaultQuery_HasNoErrors()
    {
        var result = _validator.Validate(new GetAllAuditLogsQuery());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PageBelowOne_ReturnsValidationError()
    {
        var result = _validator.Validate(new GetAllAuditLogsQuery(Page: 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllAuditLogsQuery.Page));
    }

    [Fact]
    public void Validate_PageSizeTooLarge_ReturnsValidationError()
    {
        var result = _validator.Validate(new GetAllAuditLogsQuery(PageSize: 10_000));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllAuditLogsQuery.PageSize));
    }
}

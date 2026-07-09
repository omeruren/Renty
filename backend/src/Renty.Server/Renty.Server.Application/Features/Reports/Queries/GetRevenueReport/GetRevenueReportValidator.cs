using FluentValidation;

namespace Renty.Server.Application.Features.Reports.Queries.GetRevenueReport;

public sealed class GetRevenueReportValidator : AbstractValidator<GetRevenueReportQuery>
{
    public GetRevenueReportValidator()
    {
        RuleFor(x => x.To)
            .GreaterThan(x => x.From).WithMessage("'To' must be after 'From'.")
            .When(x => x.From.HasValue && x.To.HasValue);
    }
}

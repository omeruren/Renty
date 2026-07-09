using FluentValidation;
using Renty.Server.Application.Common.Models;

namespace Renty.Server.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public sealed class GetAllAuditLogsValidator : AbstractValidator<GetAllAuditLogsQuery>
{
    public GetAllAuditLogsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {PaginationRequest.MaxPageSize}.");
    }
}

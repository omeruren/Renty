namespace Renty.Server.Application.Common.Models;

public sealed record PaginationRequest(int Page = 1, int PageSize = 10)
{
    public const int MaxPageSize = 50;
}

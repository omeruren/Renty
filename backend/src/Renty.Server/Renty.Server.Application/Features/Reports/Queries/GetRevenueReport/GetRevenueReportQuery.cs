using MediatR;
using Renty.Server.Application.Features.Reports.DTOs;

namespace Renty.Server.Application.Features.Reports.Queries.GetRevenueReport;

public sealed record GetRevenueReportQuery(DateTime? From, DateTime? To) : IRequest<RevenueReportResponse>;

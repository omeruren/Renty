using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Queries.GetAllReservations;

public sealed record GetAllReservationsQuery(
    int Page = 1,
    int PageSize = 10,
    ReservationStatus? Status = null,
    Guid? CarId = null,
    Guid? UserId = null,
    string SortBy = "startDate",
    string SortOrder = "desc") : IRequest<PagedResponse<ReservationListResponse>>;

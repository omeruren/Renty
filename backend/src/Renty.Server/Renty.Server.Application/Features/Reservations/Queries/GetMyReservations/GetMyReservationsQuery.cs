using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Queries.GetMyReservations;

public sealed record GetMyReservationsQuery(
    int Page = 1,
    int PageSize = 10,
    ReservationStatus? Status = null) : IRequest<PagedResponse<ReservationListResponse>>;

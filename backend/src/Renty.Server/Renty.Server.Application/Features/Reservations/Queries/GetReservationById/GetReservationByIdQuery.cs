using MediatR;
using Renty.Server.Application.Features.Reservations.DTOs;

namespace Renty.Server.Application.Features.Reservations.Queries.GetReservationById;

public sealed record GetReservationByIdQuery(Guid Id) : IRequest<ReservationResponse>;

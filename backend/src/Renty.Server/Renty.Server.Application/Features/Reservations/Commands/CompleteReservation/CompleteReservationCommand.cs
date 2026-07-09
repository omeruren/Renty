using MediatR;
using Renty.Server.Application.Features.Reservations.DTOs;

namespace Renty.Server.Application.Features.Reservations.Commands.CompleteReservation;

public sealed record CompleteReservationCommand(Guid Id) : IRequest<ReservationResponse>;

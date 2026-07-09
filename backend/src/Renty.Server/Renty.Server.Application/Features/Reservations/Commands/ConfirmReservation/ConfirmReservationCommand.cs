using MediatR;
using Renty.Server.Application.Features.Reservations.DTOs;

namespace Renty.Server.Application.Features.Reservations.Commands.ConfirmReservation;

public sealed record ConfirmReservationCommand(Guid Id) : IRequest<ReservationResponse>;

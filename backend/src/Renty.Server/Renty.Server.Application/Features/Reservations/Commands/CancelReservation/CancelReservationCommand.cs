using MediatR;
using Renty.Server.Application.Features.Reservations.DTOs;

namespace Renty.Server.Application.Features.Reservations.Commands.CancelReservation;

public sealed record CancelReservationCommand(Guid Id, string? Reason) : IRequest<ReservationResponse>;

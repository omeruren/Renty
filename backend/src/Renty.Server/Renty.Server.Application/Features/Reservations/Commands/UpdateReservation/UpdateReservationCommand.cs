using MediatR;
using Renty.Server.Application.Features.Reservations.DTOs;

namespace Renty.Server.Application.Features.Reservations.Commands.UpdateReservation;

public sealed record UpdateReservationCommand(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    Guid PickupLocationId,
    Guid ReturnLocationId,
    string? Notes) : IRequest<ReservationResponse>;

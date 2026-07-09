using MediatR;
using Renty.Server.Application.Features.Reservations.DTOs;

namespace Renty.Server.Application.Features.Reservations.Commands.CreateReservation;

public sealed record CreateReservationCommand(
    Guid CarId,
    DateTime StartDate,
    DateTime EndDate,
    Guid PickupLocationId,
    Guid ReturnLocationId,
    string? Notes) : IRequest<ReservationResponse>;

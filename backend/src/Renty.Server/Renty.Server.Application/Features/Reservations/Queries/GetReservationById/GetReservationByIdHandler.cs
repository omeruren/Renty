using MediatR;
using Microsoft.EntityFrameworkCore;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Application.Features.Reservations.Mappings;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Reservations.Queries.GetReservationById;

public sealed class GetReservationByIdHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetReservationByIdQuery, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        var isOwner = reservation.UserId == currentUserService.UserId;
        var isStaff = currentUserService.IsInRole("Admin") || currentUserService.IsInRole("Manager");

        if (!isOwner && !isStaff)
            throw new ForbiddenException("You can only view your own reservations.");

        return await context.Reservations
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(ReservationProjections.ToResponse)
            .FirstAsync(cancellationToken);
    }
}

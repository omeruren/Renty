using System.Linq.Expressions;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Application.Features.Reservations.Mappings;

public static class ReservationProjections
{
    public static Expression<Func<Reservation, ReservationResponse>> ToResponse { get; } =
        r => new ReservationResponse(
            r.Id, r.StartDate, r.EndDate, r.TotalPrice, r.Status,
            r.Notes, r.CancellationReason, r.CancelledAt,
            r.UserId, r.User.FirstName + " " + r.User.LastName, r.User.Email,
            r.CarId, r.Car.LicensePlate, r.Car.Brand.Name, r.Car.Model.Name,
            r.PickupLocationId, r.PickupLocation.Name,
            r.ReturnLocationId, r.ReturnLocation.Name,
            r.CreatedAt);

    public static Expression<Func<Reservation, ReservationListResponse>> ToListResponse { get; } =
        r => new ReservationListResponse(
            r.Id, r.StartDate, r.EndDate, r.TotalPrice, r.Status,
            r.UserId, r.User.FirstName + " " + r.User.LastName,
            r.CarId, r.Car.LicensePlate,
            r.PickupLocation.Name, r.ReturnLocation.Name);
}

using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class ReservationBuilder
{
    private Guid _id = Guid.NewGuid();
    private DateTime _startDate = DateTime.UtcNow.AddDays(2);
    private DateTime _endDate = DateTime.UtcNow.AddDays(5);
    private decimal _totalPrice = 2250m;
    private ReservationStatus _status = ReservationStatus.Pending;
    private Guid _userId = Guid.NewGuid();
    private Guid _carId = Guid.NewGuid();
    private Guid _pickupLocationId = Guid.NewGuid();
    private Guid _returnLocationId = Guid.NewGuid();

    public ReservationBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ReservationBuilder WithDates(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
        return this;
    }

    public ReservationBuilder WithStatus(ReservationStatus status)
    {
        _status = status;
        return this;
    }

    public ReservationBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public ReservationBuilder WithCarId(Guid carId)
    {
        _carId = carId;
        return this;
    }

    public ReservationBuilder WithPickupLocationId(Guid pickupLocationId)
    {
        _pickupLocationId = pickupLocationId;
        return this;
    }

    public ReservationBuilder WithReturnLocationId(Guid returnLocationId)
    {
        _returnLocationId = returnLocationId;
        return this;
    }

    public Reservation Build() => new()
    {
        Id = _id,
        StartDate = _startDate,
        EndDate = _endDate,
        TotalPrice = _totalPrice,
        Status = _status,
        UserId = _userId,
        CarId = _carId,
        PickupLocationId = _pickupLocationId,
        ReturnLocationId = _returnLocationId
    };
}

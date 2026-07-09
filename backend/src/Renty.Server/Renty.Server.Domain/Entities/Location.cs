using Renty.Server.Domain.Common;

namespace Renty.Server.Domain.Entities;

public sealed class Location : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Car> Cars { get; set; } = [];
    public ICollection<Reservation> PickupReservations { get; set; } = [];
    public ICollection<Reservation> ReturnReservations { get; set; } = [];
}

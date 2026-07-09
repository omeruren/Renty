using Renty.Server.Domain.Common;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Domain.Entities;

public sealed class Car : AuditableEntity
{
    public string LicensePlate { get; set; } = default!;
    public int Year { get; set; }
    public string Color { get; set; } = default!;
    public int Mileage { get; set; }
    public decimal DailyPrice { get; set; }
    public CarStatus Status { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid BrandId { get; set; }
    public Guid ModelId { get; set; }
    public Guid LocationId { get; set; }

    public Brand Brand { get; set; } = default!;
    public Model Model { get; set; } = default!;
    public Location Location { get; set; } = default!;
    public ICollection<Reservation> Reservations { get; set; } = [];
}

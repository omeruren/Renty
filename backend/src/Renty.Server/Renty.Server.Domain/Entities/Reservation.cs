using Renty.Server.Domain.Common;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Domain.Entities;

public sealed class Reservation : AuditableEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public Guid UserId { get; set; }
    public Guid CarId { get; set; }
    public Guid PickupLocationId { get; set; }
    public Guid ReturnLocationId { get; set; }

    public User User { get; set; } = default!;
    public Car Car { get; set; } = default!;
    public Location PickupLocation { get; set; } = default!;
    public Location ReturnLocation { get; set; } = default!;
}

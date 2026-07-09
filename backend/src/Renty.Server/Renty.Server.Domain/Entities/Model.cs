using Renty.Server.Domain.Common;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Domain.Entities;

public sealed class Model : AuditableEntity
{
    public string Name { get; set; } = default!;
    public VehicleCategory Category { get; set; }
    public int SeatCount { get; set; }
    public TransmissionType TransmissionType { get; set; }
    public FuelType FuelType { get; set; }
    public Guid BrandId { get; set; }

    public Brand Brand { get; set; } = default!;
    public ICollection<Car> Cars { get; set; } = [];
}

using Renty.Server.Domain.Common;

namespace Renty.Server.Domain.Entities;

public sealed class Brand : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string? LogoUrl { get; set; }

    public ICollection<Model> Models { get; set; } = [];
    public ICollection<Car> Cars { get; set; } = [];
}

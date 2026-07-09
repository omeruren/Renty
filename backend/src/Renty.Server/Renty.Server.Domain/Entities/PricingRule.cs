using Renty.Server.Domain.Common;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Domain.Entities;

public sealed class PricingRule : AuditableEntity
{
    public string Name { get; set; } = default!;
    public PricingRuleType RuleType { get; set; }
    public decimal Multiplier { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public VehicleCategory? VehicleCategory { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }
}

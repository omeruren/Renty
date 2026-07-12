using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.UnitTests.Common.Builders;

public sealed class PricingRuleBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Weekend Surcharge";
    private PricingRuleType _ruleType = PricingRuleType.WeekendMultiplier;
    private decimal _multiplier = 1.2m;
    private bool _isActive = true;
    private int _priority = 0;

    public PricingRuleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PricingRuleBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public PricingRuleBuilder WithMultiplier(decimal multiplier)
    {
        _multiplier = multiplier;
        return this;
    }

    public PricingRule Build() => new()
    {
        Id = _id,
        Name = _name,
        RuleType = _ruleType,
        Multiplier = _multiplier,
        IsActive = _isActive,
        Priority = _priority
    };
}

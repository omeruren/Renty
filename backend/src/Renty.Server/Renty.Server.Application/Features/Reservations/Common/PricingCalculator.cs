using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Reservations.Common;

/// <summary>
/// Applies R-RES-010 (price based on duration and applicable pricing rules). A rule applies when
/// active, its VehicleCategory is unset or matches the car's, and the reservation's start date falls
/// inside its validity window. Matching rules compound onto the base price in Priority-descending order.
/// </summary>
public static class PricingCalculator
{
    public static decimal CalculateTotalPrice(
        decimal dailyPrice,
        int days,
        DateTime startDate,
        VehicleCategory carCategory,
        IEnumerable<PricingRule> activeRules)
    {
        var basePrice = dailyPrice * days;

        var applicableRules = activeRules
            .Where(r => r.IsActive
                && (r.VehicleCategory is null || r.VehicleCategory == carCategory)
                && startDate >= (r.StartDate ?? DateTime.MinValue)
                && startDate <= (r.EndDate ?? DateTime.MaxValue))
            .OrderByDescending(r => r.Priority);

        return applicableRules.Aggregate(basePrice, (price, rule) => price * rule.Multiplier);
    }
}

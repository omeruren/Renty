using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Persistence.Configurations;

public sealed class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.RuleType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.VehicleCategory)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Multiplier)
            .HasPrecision(8, 4);

        builder.HasIndex(p => p.IsActive);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_PricingRule_Multiplier", "[Multiplier] > 0");
            t.HasCheckConstraint("CK_PricingRule_Priority", "[Priority] >= 0");
        });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

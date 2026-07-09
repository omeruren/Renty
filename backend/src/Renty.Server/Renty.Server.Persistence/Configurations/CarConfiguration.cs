using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Persistence.Configurations;

public sealed class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.LicensePlate)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.DailyPrice)
            .HasPrecision(18, 2);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(c => c.LicensePlate)
            .IsUnique();

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.BrandId);
        builder.HasIndex(c => c.LocationId);

        builder.HasOne(c => c.Brand)
            .WithMany(b => b.Cars)
            .HasForeignKey(c => c.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Model)
            .WithMany(m => m.Cars)
            .HasForeignKey(c => c.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Location)
            .WithMany(l => l.Cars)
            .HasForeignKey(c => c.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Car_DailyPrice", "[DailyPrice] > 0");
            t.HasCheckConstraint("CK_Car_Year", "[Year] >= 1900 AND [Year] <= YEAR(GETUTCDATE()) + 1");
            t.HasCheckConstraint("CK_Car_Mileage", "[Mileage] >= 0");
        });

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

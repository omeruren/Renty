using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Persistence.Configurations;

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.District)
            .HasMaxLength(100);

        builder.Property(l => l.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(l => l.Email)
            .HasMaxLength(256);

        builder.HasIndex(l => l.IsActive);
        builder.HasIndex(l => l.City);

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}

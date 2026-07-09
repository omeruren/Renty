using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Persistence.Configurations;

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.HasIndex(b => b.Name)
            .IsUnique();

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}

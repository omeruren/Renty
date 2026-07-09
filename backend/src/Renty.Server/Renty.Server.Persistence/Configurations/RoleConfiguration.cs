using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renty.Server.Domain.Entities;
using Renty.Server.Persistence.Seed;

namespace Renty.Server.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(256);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasData(SeedData.Roles);
    }
}

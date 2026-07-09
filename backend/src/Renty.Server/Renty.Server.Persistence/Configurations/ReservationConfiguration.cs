using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renty.Server.Domain.Entities;

namespace Renty.Server.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.Property(r => r.CancellationReason)
            .HasMaxLength(500);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CarId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.StartDate, r.EndDate });

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Car)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PickupLocation)
            .WithMany(l => l.PickupReservations)
            .HasForeignKey(r => r.PickupLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReturnLocation)
            .WithMany(l => l.ReturnReservations)
            .HasForeignKey(r => r.ReturnLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Reservation_Dates", "[EndDate] > [StartDate]");
            t.HasCheckConstraint("CK_Reservation_TotalPrice", "[TotalPrice] >= 0");
        });

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

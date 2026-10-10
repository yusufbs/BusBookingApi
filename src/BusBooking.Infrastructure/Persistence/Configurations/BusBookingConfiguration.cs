using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Infrastructure.Persistence.Configurations;

public sealed class BusBookingConfiguration : IEntityTypeConfiguration<Domain.Entities.BusBooking>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.BusBooking> builder)
    {
        builder.ToTable("BusBookings");

        builder.HasKey(b => b.BookingId);

        builder.Property(b => b.BookingDate)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(b => b.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.BookingStatus)
            .HasMaxLength(30)
            .HasDefaultValue("Confirmed")
            .IsRequired();

        // Foreign keys
        builder.HasOne(b => b.Customer)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CustId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Schedule)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(b => b.CustId)
            .HasDatabaseName("IX_BusBookings_CustId");

        builder.HasIndex(b => new { b.ScheduleId, b.BookingStatus })
            .HasDatabaseName("IX_BusBookings_Schedule_Status");
    }
}

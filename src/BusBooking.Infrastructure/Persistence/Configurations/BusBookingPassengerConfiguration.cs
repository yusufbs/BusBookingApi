using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Infrastructure.Persistence.Configurations;

public sealed class BusBookingPassengerConfiguration : IEntityTypeConfiguration<BusBookingPassenger>
{
    public void Configure(EntityTypeBuilder<BusBookingPassenger> builder)
    {
        builder.ToTable("BusBookingPassengers");

        builder.HasKey(p => p.PassengerId);

        builder.Property(p => p.PassengerName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Age)
            .IsRequired();

        builder.Property(p => p.Gender)
            .HasMaxLength(10)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.SeatNo)
            .IsRequired();

        builder.HasOne(p => p.Booking)
            .WithMany(b => b.Passengers)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.BookingId, p.SeatNo })
            .HasDatabaseName("IX_BusBookingPassengers_Booking_Seat");
    }
}

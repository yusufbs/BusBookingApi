using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Infrastructure.Persistence.Configurations;

public sealed class BusScheduleConfiguration : IEntityTypeConfiguration<BusSchedule>
{
    public void Configure(EntityTypeBuilder<BusSchedule> builder)
    {
        builder.ToTable("BusSchedules");

        builder.HasKey(s => s.ScheduleId);

        builder.Property(s => s.BusName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.BusVehicleNo)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(s => s.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.TotalSeats)
            .IsRequired();

        builder.Property(s => s.DepartureTime)
            .IsRequired();

        builder.Property(s => s.ArrivalTime)
            .IsRequired();

        builder.Property(s => s.ScheduleDate)
            .IsRequired();

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        // Foreign keys
        builder.HasOne(s => s.Vendor)
            .WithMany(v => v.Schedules)
            .HasForeignKey(s => s.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.FromLocationNav)
            .WithMany(l => l.DepartureSchedules)
            .HasForeignKey(s => s.FromLocation)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ToLocationNav)
            .WithMany(l => l.ArrivalSchedules)
            .HasForeignKey(s => s.ToLocation)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.VendorId)
            .HasDatabaseName("IX_BusSchedules_VendorId");

        // High-performance search covering index
        builder.HasIndex(s => new { s.FromLocation, s.ToLocation, s.ScheduleDate })
            .IncludeProperties(s => new
            {
                s.ScheduleId,
                s.VendorId,
                s.BusName,
                s.BusVehicleNo,
                s.DepartureTime,
                s.ArrivalTime,
                s.Price,
                s.TotalSeats
            })
            .HasDatabaseName("IX_BusSchedules_Search_Covering");
    }
}

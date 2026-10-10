using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Infrastructure.Persistence.Configurations;

public sealed class BusAddressLocationConfiguration : IEntityTypeConfiguration<BusAddressLocation>
{
    public void Configure(EntityTypeBuilder<BusAddressLocation> builder)
    {
        builder.ToTable("BusAddressLocations");

        builder.HasKey(al => al.LocationPointId);

        builder.Property(al => al.LocationTitle)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(al => al.LocationAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(al => al.Location)
            .WithMany(l => l.AddressLocations)
            .HasForeignKey(al => al.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(al => al.LocationId)
            .HasDatabaseName("IX_BusAddressLocations_LocationId");
    }
}

using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Infrastructure.Persistence.Configurations;

public sealed class BusLocationConfiguration : IEntityTypeConfiguration<BusLocation>
{
    public void Configure(EntityTypeBuilder<BusLocation> builder)
    {
        builder.ToTable("BusLocations");

        builder.HasKey(l => l.LocationId);

        builder.Property(l => l.LocationName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Code)
            .HasMaxLength(10)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(l => l.Code)
            .IsUnique()
            .HasDatabaseName("IX_BusLocations_Code");
    }
}

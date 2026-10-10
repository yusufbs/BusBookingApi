using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Infrastructure.Persistence.Configurations;

public sealed class BusVendorConfiguration : IEntityTypeConfiguration<BusVendor>
{
    public void Configure(EntityTypeBuilder<BusVendor> builder)
    {
        builder.ToTable("BusVendors");

        builder.HasKey(v => v.VendorId);

        builder.Property(v => v.VendorName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(v => v.ContactNo)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(v => v.EmailId)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(v => v.IsActive)
            .HasDefaultValue(true)
            .IsRequired();
    }
}

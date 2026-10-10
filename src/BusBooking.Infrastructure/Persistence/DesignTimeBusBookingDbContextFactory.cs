using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BusBooking.Infrastructure.Persistence;

public sealed class DesignTimeBusBookingDbContextFactory : IDesignTimeDbContextFactory<BusBookingDbContext>
{
    public BusBookingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BusBookingDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=BusBookingDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new BusBookingDbContext(optionsBuilder.Options);
    }
}

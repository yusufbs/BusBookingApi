using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BusBooking.Infrastructure.Persistence;

public sealed class BusBookingDbContext : DbContext, IBusBookingDbContext
{
    public BusBookingDbContext(DbContextOptions<BusBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<BusVendor> BusVendors => Set<BusVendor>();
    public DbSet<BusLocation> BusLocations => Set<BusLocation>();
    public DbSet<BusAddressLocation> BusAddressLocations => Set<BusAddressLocation>();
    public DbSet<BusSchedule> BusSchedules => Set<BusSchedule>();
    public DbSet<Domain.Entities.BusBooking> BusBookings => Set<Domain.Entities.BusBooking>();
    public DbSet<BusBookingPassenger> BusBookingPassengers => Set<BusBookingPassenger>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusBookingDbContext).Assembly);
    }
}

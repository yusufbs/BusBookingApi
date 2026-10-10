using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BusBooking.Application.Common.Interfaces;

public interface IBusBookingDbContext
{
    DbSet<User> Users { get; }
    DbSet<BusVendor> BusVendors { get; }
    DbSet<BusLocation> BusLocations { get; }
    DbSet<BusAddressLocation> BusAddressLocations { get; }
    DbSet<BusSchedule> BusSchedules { get; }
    DbSet<Domain.Entities.BusBooking> BusBookings { get; }
    DbSet<BusBookingPassenger> BusBookingPassengers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

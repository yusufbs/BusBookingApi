using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.DTOs;
using BusBooking.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Features.Schedules;

public sealed record GetBusSchedulesByVendorQuery(int VendorId) : IRequest<IReadOnlyList<BusScheduleDetailsDto>>;

public sealed class GetBusSchedulesByVendorQueryHandler : IRequestHandler<GetBusSchedulesByVendorQuery, IReadOnlyList<BusScheduleDetailsDto>>
{
    private readonly IBusBookingDbContext _context;

    public GetBusSchedulesByVendorQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<IReadOnlyList<BusScheduleDetailsDto>> Handle(GetBusSchedulesByVendorQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusSchedules
            .AsNoTracking()
            .Where(s => s.VendorId == request.VendorId)
            .Select(s => new BusScheduleDetailsDto
            {
                ScheduleId = s.ScheduleId,
                VendorId = s.VendorId,
                VendorName = s.Vendor.VendorName,
                BusName = s.BusName,
                BusVehicleNo = s.BusVehicleNo,
                FromLocationName = s.FromLocationNav.LocationName,
                ToLocationName = s.ToLocationNav.LocationName,
                DepartureTime = s.DepartureTime.ToString("HH:mm:ss"),
                ArrivalTime = s.ArrivalTime.ToString("HH:mm:ss"),
                ScheduleDate = s.ScheduleDate.ToString("yyyy-MM-dd"),
                Price = s.Price,
                TotalSeats = s.TotalSeats,
                AvailableSeats = s.TotalSeats - s.Bookings
                    .Where(b => b.BookingStatus == "Confirmed")
                    .SelectMany(b => b.Passengers)
                    .Count()
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed record GetBusScheduleByIdQuery(int Id) : IRequest<BusScheduleDetailsDto?>;

public sealed class GetBusScheduleByIdQueryHandler : IRequestHandler<GetBusScheduleByIdQuery, BusScheduleDetailsDto?>
{
    private readonly IBusBookingDbContext _context;

    public GetBusScheduleByIdQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<BusScheduleDetailsDto?> Handle(GetBusScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusSchedules
            .AsNoTracking()
            .Where(s => s.ScheduleId == request.Id)
            .Select(s => new BusScheduleDetailsDto
            {
                ScheduleId = s.ScheduleId,
                VendorId = s.VendorId,
                VendorName = s.Vendor.VendorName,
                BusName = s.BusName,
                BusVehicleNo = s.BusVehicleNo,
                FromLocationName = s.FromLocationNav.LocationName,
                ToLocationName = s.ToLocationNav.LocationName,
                DepartureTime = s.DepartureTime.ToString("HH:mm:ss"),
                ArrivalTime = s.ArrivalTime.ToString("HH:mm:ss"),
                ScheduleDate = s.ScheduleDate.ToString("yyyy-MM-dd"),
                Price = s.Price,
                TotalSeats = s.TotalSeats,
                AvailableSeats = s.TotalSeats - s.Bookings
                    .Where(b => b.BookingStatus == "Confirmed")
                    .SelectMany(b => b.Passengers)
                    .Count()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed record CreateBusScheduleCommand(
    int VendorId,
    string BusName,
    string BusVehicleNo,
    int FromLocation,
    int ToLocation,
    string DepartureTime,
    string ArrivalTime,
    string ScheduleDate,
    decimal Price,
    int TotalSeats) : IRequest<int>;

public sealed class CreateBusScheduleCommandHandler : IRequestHandler<CreateBusScheduleCommand, int>
{
    private readonly IBusBookingDbContext _context;

    public CreateBusScheduleCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<int> Handle(CreateBusScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = new BusSchedule
        {
            VendorId = request.VendorId,
            BusName = request.BusName,
            BusVehicleNo = request.BusVehicleNo,
            FromLocation = request.FromLocation,
            ToLocation = request.ToLocation,
            DepartureTime = TimeOnly.Parse(request.DepartureTime),
            ArrivalTime = TimeOnly.Parse(request.ArrivalTime),
            ScheduleDate = DateOnly.Parse(request.ScheduleDate),
            Price = request.Price,
            TotalSeats = request.TotalSeats
        };

        _context.BusSchedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        return schedule.ScheduleId;
    }
}

public sealed record UpdateBusScheduleCommand(
    int Id,
    int VendorId,
    string BusName,
    string BusVehicleNo,
    int FromLocation,
    int ToLocation,
    string DepartureTime,
    string ArrivalTime,
    string ScheduleDate,
    decimal Price,
    int TotalSeats) : IRequest<bool>;

public sealed class UpdateBusScheduleCommandHandler : IRequestHandler<UpdateBusScheduleCommand, bool>
{
    private readonly IBusBookingDbContext _context;

    public UpdateBusScheduleCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateBusScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.BusSchedules
            .FirstOrDefaultAsync(s => s.ScheduleId == request.Id, cancellationToken);

        if (schedule is null) return false;

        schedule.VendorId = request.VendorId;
        schedule.BusName = request.BusName;
        schedule.BusVehicleNo = request.BusVehicleNo;
        schedule.FromLocation = request.FromLocation;
        schedule.ToLocation = request.ToLocation;
        schedule.DepartureTime = TimeOnly.Parse(request.DepartureTime);
        schedule.ArrivalTime = TimeOnly.Parse(request.ArrivalTime);
        schedule.ScheduleDate = DateOnly.Parse(request.ScheduleDate);
        schedule.Price = request.Price;
        schedule.TotalSeats = request.TotalSeats;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record DeleteBusScheduleCommand(int Id) : IRequest<bool>;

public sealed class DeleteBusScheduleCommandHandler : IRequestHandler<DeleteBusScheduleCommand, bool>
{
    private readonly IBusBookingDbContext _context;

    public DeleteBusScheduleCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteBusScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.BusSchedules
            .FirstOrDefaultAsync(s => s.ScheduleId == request.Id, cancellationToken);

        if (schedule is null) return false;

        _context.BusSchedules.Remove(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

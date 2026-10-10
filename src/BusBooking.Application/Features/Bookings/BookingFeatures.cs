using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Features.Bookings;

public sealed record GetAllBusBookingsQuery(int? VendorId = null) : IRequest<IReadOnlyList<BusBookingListDto>>;

public sealed class GetAllBusBookingsQueryHandler : IRequestHandler<GetAllBusBookingsQuery, IReadOnlyList<BusBookingListDto>>
{
    private readonly IBusBookingDbContext _context;

    public GetAllBusBookingsQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<IReadOnlyList<BusBookingListDto>> Handle(GetAllBusBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BusBookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Schedule)
                .ThenInclude(s => s.Vendor)
            .Include(b => b.Schedule)
                .ThenInclude(s => s.FromLocationNav)
            .Include(b => b.Schedule)
                .ThenInclude(s => s.ToLocationNav)
            .Include(b => b.Passengers)
            .Where(b => b.BookingStatus == "Confirmed");

        if (request.VendorId.HasValue && request.VendorId.Value > 0)
        {
            query = query.Where(b => b.Schedule.VendorId == request.VendorId.Value);
        }

        return await query
            .Select(b => new BusBookingListDto
            {
                BookingId = b.BookingId,
                BusName = b.Schedule.BusName,
                FromLocation = b.Schedule.FromLocationNav.LocationName,
                ToLocation = b.Schedule.ToLocationNav.LocationName,
                TravelDate = b.Schedule.ScheduleDate.ToString("yyyy-MM-dd"),
                VendorName = b.Schedule.Vendor.VendorName,
                CustomerName = b.Customer.FullName,
                EmailId = b.Customer.EmailId,
                BusVehicleNo = b.Schedule.BusVehicleNo,
                SeatNos = b.Passengers.Select(p => p.SeatNo).ToList(),
                VendorId = b.Schedule.VendorId
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed record GetBusBookingByIdQuery(int Id) : IRequest<BusBookingDto?>;

public sealed class GetBusBookingByIdQueryHandler : IRequestHandler<GetBusBookingByIdQuery, BusBookingDto?>
{
    private readonly IBusBookingDbContext _context;

    public GetBusBookingByIdQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<BusBookingDto?> Handle(GetBusBookingByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusBookings
            .AsNoTracking()
            .Where(b => b.BookingId == request.Id)
            .Select(b => new BusBookingDto
            {
                BookingId = b.BookingId,
                CustId = b.CustId,
                ScheduleId = b.ScheduleId,
                BookingDate = b.BookingDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                TotalAmount = b.TotalAmount,
                BookingStatus = b.BookingStatus
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed record DeleteBusBookingCommand(int Id) : IRequest<bool>;

public sealed class DeleteBusBookingCommandHandler : IRequestHandler<DeleteBusBookingCommand, bool>
{
    private readonly IBusBookingDbContext _context;

    public DeleteBusBookingCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteBusBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.BusBookings
            .FirstOrDefaultAsync(b => b.BookingId == request.Id, cancellationToken);

        if (booking is null) return false;

        // Soft delete / cancel status to release seats
        booking.BookingStatus = "Cancelled";
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

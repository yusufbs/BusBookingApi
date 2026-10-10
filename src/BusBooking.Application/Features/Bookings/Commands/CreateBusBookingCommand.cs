using BusBooking.Application.Common.Exceptions;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.DTOs;
using BusBooking.Domain.Entities;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Features.Bookings.Commands;

public sealed record CreateBusBookingCommand(
    int CustId,
    int ScheduleId,
    IReadOnlyList<BusBookingPassengerDto> Passengers) : IRequest<BusBookingDto>;

public sealed class CreateBusBookingCommandHandler : IRequestHandler<CreateBusBookingCommand, BusBookingDto>
{
    private readonly IBusBookingDbContext _context;

    public CreateBusBookingCommandHandler(IBusBookingDbContext context)
    {
        _context = context;
    }

    public async Task<BusBookingDto> Handle(CreateBusBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate customer existence
        var customerExists = await _context.Users
            .AnyAsync(u => u.UserId == request.CustId, cancellationToken);
        if (!customerExists)
        {
            throw new NotFoundException("User", request.CustId);
        }

        // 2. Validate schedule existence
        var schedule = await _context.BusSchedules
            .FirstOrDefaultAsync(s => s.ScheduleId == request.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            throw new NotFoundException("BusSchedule", request.ScheduleId);
        }

        var requestedSeats = request.Passengers.Select(p => p.SeatNo).Distinct().ToList();

        // Validate seats do not exceed bus capacity
        var invalidSeat = requestedSeats.FirstOrDefault(s => s > schedule.TotalSeats);
        if (invalidSeat > 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "SeatNo", [$"Seat number {invalidSeat} exceeds total available bus capacity of {schedule.TotalSeats}."] }
            });
        }

        // 3. Atomic transaction with dual-layer concurrency protection
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Layer 1: Read check within transaction
            var takenSeats = await _context.BusBookingPassengers
                .Where(p => p.Booking.ScheduleId == request.ScheduleId 
                            && p.Booking.BookingStatus == BookingStatus.Confirmed 
                            && requestedSeats.Contains(p.SeatNo))
                .Select(p => p.SeatNo)
                .ToListAsync(cancellationToken);

            if (takenSeats.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                BusBooking.Application.Common.Diagnostics.BusBookingMetrics.BookingConflictsCounter.Add(1);
                throw new SeatAlreadyBookedException(request.ScheduleId, takenSeats);
            }

            // Calculate total booking price
            var totalAmount = schedule.Price * request.Passengers.Count;

            var booking = new Domain.Entities.BusBooking
            {
                CustId = request.CustId,
                ScheduleId = request.ScheduleId,
                BookingDate = DateTimeOffset.UtcNow,
                TotalAmount = totalAmount,
                BookingStatus = BookingStatus.Confirmed
            };

            foreach (var passengerDto in request.Passengers)
            {
                booking.Passengers.Add(new BusBookingPassenger
                {
                    PassengerName = passengerDto.PassengerName,
                    Age = passengerDto.Age,
                    Gender = passengerDto.Gender,
                    SeatNo = passengerDto.SeatNo
                });
            }

            _context.BusBookings.Add(booking);

            // Layer 2: SaveChanges will also trigger the unique clustered index on vw_ActiveScheduleSeats
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            BusBooking.Application.Common.Diagnostics.BusBookingMetrics.BookingsCreatedCounter.Add(1);
            BusBooking.Application.Common.Diagnostics.BusBookingMetrics.SeatsBookedCounter.Add(request.Passengers.Count);

            return new BusBookingDto
            {
                BookingId = booking.BookingId,
                CustId = booking.CustId,
                ScheduleId = booking.ScheduleId,
                BookingDate = booking.BookingDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                TotalAmount = booking.TotalAmount,
                BookingStatus = booking.BookingStatus
            };
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            // If a concurrent transaction inserted the same seat simultaneously, the unique constraint catches it
            if (ex.InnerException?.Message.Contains("UQ_vw_ActiveScheduleSeats") == true ||
                ex.Message.Contains("UQ_vw_ActiveScheduleSeats"))
            {
                BusBooking.Application.Common.Diagnostics.BusBookingMetrics.BookingConflictsCounter.Add(1);
                throw new SeatAlreadyBookedException(request.ScheduleId, requestedSeats);
            }

            throw;
        }
    }
}

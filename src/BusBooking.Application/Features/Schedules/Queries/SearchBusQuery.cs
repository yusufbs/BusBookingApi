using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.DTOs;
using Dapper;
using MediatR;

namespace BusBooking.Application.Features.Schedules.Queries;

public sealed record SearchBusQuery(int FromLocation, int ToLocation, string TravelDate) 
    : IRequest<IReadOnlyList<BusScheduleDetailsDto>>;

public sealed class SearchBusQueryHandler : IRequestHandler<SearchBusQuery, IReadOnlyList<BusScheduleDetailsDto>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SearchBusQueryHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<BusScheduleDetailsDto>> Handle(SearchBusQuery request, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        const string sql = """
            SELECT 
                s.ScheduleId,
                s.VendorId,
                v.VendorName,
                s.BusName,
                s.BusVehicleNo,
                fl.LocationName AS FromLocationName,
                tl.LocationName AS ToLocationName,
                CONVERT(VARCHAR(8), s.DepartureTime, 108) AS DepartureTime,
                CONVERT(VARCHAR(8), s.ArrivalTime, 108) AS ArrivalTime,
                CONVERT(VARCHAR(10), s.ScheduleDate, 23) AS ScheduleDate,
                s.Price,
                s.TotalSeats,
                CAST((s.TotalSeats - ISNULL(booked.BookedCount, 0)) AS INT) AS AvailableSeats
            FROM dbo.BusSchedules s WITH (NOLOCK)
            INNER JOIN dbo.BusVendors v WITH (NOLOCK) ON s.VendorId = v.VendorId
            INNER JOIN dbo.BusLocations fl WITH (NOLOCK) ON s.FromLocation = fl.LocationId
            INNER JOIN dbo.BusLocations tl WITH (NOLOCK) ON s.ToLocation = tl.LocationId
            OUTER APPLY (
                SELECT COUNT(p.PassengerId) AS BookedCount
                FROM dbo.BusBookings b WITH (NOLOCK)
                INNER JOIN dbo.BusBookingPassengers p WITH (NOLOCK) ON b.BookingId = p.BookingId
                WHERE b.ScheduleId = s.ScheduleId AND b.BookingStatus = 'Confirmed'
            ) booked
            WHERE s.FromLocation = @FromLocation
              AND s.ToLocation = @ToLocation
              AND s.ScheduleDate = @TravelDate
              AND v.IsActive = 1
            ORDER BY s.DepartureTime ASC;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<BusScheduleDetailsDto>(
            new CommandDefinition(sql, new { request.FromLocation, request.ToLocation, request.TravelDate }, cancellationToken: cancellationToken));

        sw.Stop();
        BusBooking.Application.Common.Diagnostics.BusBookingMetrics.SearchDurationHistogram.Record(sw.Elapsed.TotalMilliseconds);

        return results.AsList();
    }
}

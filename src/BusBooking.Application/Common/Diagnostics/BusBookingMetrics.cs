using System.Diagnostics.Metrics;

namespace BusBooking.Application.Common.Diagnostics;

public static class BusBookingMetrics
{
    public const string MeterName = "BusBooking.Monitoring";

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> BookingsCreatedCounter = 
        Meter.CreateCounter<long>("bus_bookings_created_total", "bookings", "Total number of bus bookings created successfully");

    public static readonly Counter<long> SeatsBookedCounter = 
        Meter.CreateCounter<long>("bus_seats_booked_total", "seats", "Total number of bus seats booked");

    public static readonly Counter<long> BookingConflictsCounter = 
        Meter.CreateCounter<long>("bus_booking_conflicts_total", "conflicts", "Total number of seat booking race conditions/conflicts encountered");

    public static readonly Histogram<double> SearchDurationHistogram = 
        Meter.CreateHistogram<double>("bus_search_duration_ms", "ms", "Latency of bus search queries");
}

namespace BusBooking.Domain.Entities;

public sealed class BusBookingPassenger
{
    public int PassengerId { get; set; }
    public int BookingId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int SeatNo { get; set; }

    // Navigation
    public BusBooking Booking { get; set; } = null!;
}

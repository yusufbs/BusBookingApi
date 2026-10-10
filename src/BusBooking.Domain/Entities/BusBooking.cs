namespace BusBooking.Domain.Entities;

public sealed class BusBooking
{
    public int BookingId { get; set; }
    public int CustId { get; set; }
    public int ScheduleId { get; set; }
    public DateTimeOffset BookingDate { get; set; } = DateTimeOffset.UtcNow;
    public decimal TotalAmount { get; set; }
    public string BookingStatus { get; set; } = "Confirmed";

    // Navigation
    public User Customer { get; set; } = null!;
    public BusSchedule Schedule { get; set; } = null!;
    public ICollection<BusBookingPassenger> Passengers { get; set; } = new List<BusBookingPassenger>();
}

namespace BusBooking.Domain.Entities;

public sealed class BusSchedule
{
    public int ScheduleId { get; set; }
    public int VendorId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string BusVehicleNo { get; set; } = string.Empty;
    public int FromLocation { get; set; }
    public int ToLocation { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public DateOnly ScheduleDate { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public BusVendor Vendor { get; set; } = null!;
    public BusLocation FromLocationNav { get; set; } = null!;
    public BusLocation ToLocationNav { get; set; } = null!;
    public ICollection<BusBooking> Bookings { get; set; } = new List<BusBooking>();
}

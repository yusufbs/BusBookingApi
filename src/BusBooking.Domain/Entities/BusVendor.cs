namespace BusBooking.Domain.Entities;

public sealed class BusVendor
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string ContactNo { get; set; } = string.Empty;
    public string EmailId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<BusSchedule> Schedules { get; set; } = new List<BusSchedule>();
}

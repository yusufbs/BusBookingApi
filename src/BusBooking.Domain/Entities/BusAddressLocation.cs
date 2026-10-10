namespace BusBooking.Domain.Entities;

public sealed class BusAddressLocation
{
    public int LocationPointId { get; set; }
    public int LocationId { get; set; }
    public string LocationTitle { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;

    // Navigation
    public BusLocation Location { get; set; } = null!;
}

namespace BusBooking.Domain.Entities;

public sealed class BusLocation
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    // Navigation
    public ICollection<BusAddressLocation> AddressLocations { get; set; } = new List<BusAddressLocation>();
    public ICollection<BusSchedule> DepartureSchedules { get; set; } = new List<BusSchedule>();
    public ICollection<BusSchedule> ArrivalSchedules { get; set; } = new List<BusSchedule>();
}

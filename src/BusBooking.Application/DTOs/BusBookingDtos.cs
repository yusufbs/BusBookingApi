namespace BusBooking.Application.DTOs;

public sealed record BusScheduleDetailsDto
{
    public int ScheduleId { get; init; }
    public int VendorId { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public string BusName { get; init; } = string.Empty;
    public string BusVehicleNo { get; init; } = string.Empty;
    public string FromLocationName { get; init; } = string.Empty;
    public string ToLocationName { get; init; } = string.Empty;
    public string DepartureTime { get; init; } = string.Empty;
    public string ArrivalTime { get; init; } = string.Empty;
    public string ScheduleDate { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int TotalSeats { get; init; }
    public int AvailableSeats { get; init; }
}

public sealed record AvailableRouteDto
{
    public string FromLocationName { get; init; } = string.Empty;
    public string ToLocationName { get; init; } = string.Empty;
    public string ScheduleDate { get; init; } = string.Empty;
    public int BusCount { get; init; }
}

public sealed record BusBookingDto
{
    public int BookingId { get; init; }
    public int CustId { get; init; }
    public int ScheduleId { get; init; }
    public string BookingDate { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string BookingStatus { get; init; } = "Confirmed";
}

public sealed record BusBookingPassengerDto
{
    public int PassengerId { get; init; }
    public int BookingId { get; init; }
    public string PassengerName { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public int SeatNo { get; init; }
}

public sealed record BusBookingViewModel
{
    public int BookingId { get; init; }
    public int CustId { get; init; }
    public int ScheduleId { get; init; }
    public string BookingDate { get; init; } = string.Empty;
    public List<BusBookingPassengerDto> BusBookingPassengers { get; init; } = [];
}

public sealed record BusBookingListDto
{
    public int BookingId { get; init; }
    public string BusName { get; init; } = string.Empty;
    public string FromLocation { get; init; } = string.Empty;
    public string ToLocation { get; init; } = string.Empty;
    public string TravelDate { get; init; } = string.Empty;
    public string VendorName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string EmailId { get; init; } = string.Empty;
    public string BusVehicleNo { get; init; } = string.Empty;
    public List<int> SeatNos { get; init; } = [];
    public int VendorId { get; init; }
}

public sealed record BusVendorDto
{
    public int VendorId { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public string ContactNo { get; init; } = string.Empty;
    public string EmailId { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}

public sealed record BusLocationDto
{
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

public sealed record BusAddressLocationDto
{
    public int LocationPointId { get; init; }
    public int LocationId { get; init; }
    public string LocationTitle { get; init; } = string.Empty;
    public string LocationAddress { get; init; } = string.Empty;
}

public sealed record UserDto
{
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string EmailId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = "Customer";
    public string Password { get; init; } = string.Empty;
    public string CreatedDate { get; init; } = string.Empty;
    public string? ProjectName { get; init; }
    public string? Token { get; init; }
    public string? RefreshToken { get; init; }
    public string? RefreshTokenExpiryTime { get; init; }
}

public sealed record LoginUserDto
{
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

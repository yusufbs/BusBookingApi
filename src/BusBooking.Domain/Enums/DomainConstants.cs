namespace BusBooking.Domain.Enums;

public static class UserRole
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Associate = "Associate";
    public const string Customer = "Customer";

    public static readonly string[] AllRoles = [Admin, Manager, Associate, Customer];
}

public static class BookingStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
}

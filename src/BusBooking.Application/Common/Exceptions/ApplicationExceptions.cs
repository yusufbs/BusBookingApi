namespace BusBooking.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key ({key}) was not found.")
    {
    }
}

public class SeatAlreadyBookedException : Exception
{
    public IReadOnlyCollection<int> ConflictingSeats { get; }

    public SeatAlreadyBookedException(int scheduleId, IEnumerable<int> seats)
        : base($"One or more selected seats [{string.Join(", ", seats)}] are already booked for schedule {scheduleId}.")
    {
        ConflictingSeats = seats.ToArray();
    }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}

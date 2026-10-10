namespace BusBooking.Application.Common.Models;

public sealed record ApiResponse<T>
{
    public string Message { get; init; } = string.Empty;
    public bool Result { get; init; } = true;
    public T? Data { get; init; }

    public static ApiResponse<T> Success(T data, string message = "Success") =>
        new() { Result = true, Message = message, Data = data };

    public static ApiResponse<T> Failure(string message, T? data = default) =>
        new() { Result = false, Message = message, Data = data };
}

public sealed record ApiResponse
{
    public string Message { get; init; } = string.Empty;
    public bool Result { get; init; } = true;
    public object? Data { get; init; }

    public static ApiResponse Success(object? data = null, string message = "Success") =>
        new() { Result = true, Message = message, Data = data };

    public static ApiResponse Failure(string message, object? data = null) =>
        new() { Result = false, Message = message, Data = data };
}

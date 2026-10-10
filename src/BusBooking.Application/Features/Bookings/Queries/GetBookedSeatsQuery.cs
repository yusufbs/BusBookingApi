using BusBooking.Application.Common.Interfaces;
using Dapper;
using MediatR;

namespace BusBooking.Application.Features.Bookings.Queries;

public sealed record GetBookedSeatsQuery(int ScheduleId) : IRequest<IReadOnlyList<int>>;

public sealed class GetBookedSeatsQueryHandler : IRequestHandler<GetBookedSeatsQuery, IReadOnlyList<int>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GetBookedSeatsQueryHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<int>> Handle(GetBookedSeatsQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT p.SeatNo
            FROM dbo.BusBookingPassengers p WITH (NOLOCK)
            INNER JOIN dbo.BusBookings b WITH (NOLOCK) ON p.BookingId = b.BookingId
            WHERE b.ScheduleId = @ScheduleId
              AND b.BookingStatus = 'Confirmed'
            ORDER BY p.SeatNo ASC;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var seats = await connection.QueryAsync<int>(
            new CommandDefinition(sql, new { request.ScheduleId }, cancellationToken: cancellationToken));

        return seats.AsList();
    }
}

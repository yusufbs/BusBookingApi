using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.DTOs;
using Dapper;
using MediatR;

namespace BusBooking.Application.Features.Schedules.Queries;

public sealed record GetAvailableRoutesQuery : IRequest<IReadOnlyList<AvailableRouteDto>>;

public sealed class GetAvailableRoutesQueryHandler : IRequestHandler<GetAvailableRoutesQuery, IReadOnlyList<AvailableRouteDto>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GetAvailableRoutesQueryHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AvailableRouteDto>> Handle(GetAvailableRoutesQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT 
                fl.LocationName AS FromLocationName,
                tl.LocationName AS ToLocationName,
                CONVERT(VARCHAR(10), s.ScheduleDate, 23) AS ScheduleDate,
                COUNT(s.ScheduleId) AS BusCount
            FROM dbo.BusSchedules s WITH (NOLOCK)
            INNER JOIN dbo.BusLocations fl WITH (NOLOCK) ON s.FromLocation = fl.LocationId
            INNER JOIN dbo.BusLocations tl WITH (NOLOCK) ON s.ToLocation = tl.LocationId
            GROUP BY fl.LocationName, tl.LocationName, s.ScheduleDate
            ORDER BY s.ScheduleDate ASC, fl.LocationName ASC;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<AvailableRouteDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return results.AsList();
    }
}

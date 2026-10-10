using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.DTOs;
using BusBooking.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Features.Locations;

public sealed record GetBusLocationsQuery : IRequest<IReadOnlyList<BusLocationDto>>;

public sealed class GetBusLocationsQueryHandler : IRequestHandler<GetBusLocationsQuery, IReadOnlyList<BusLocationDto>>
{
    private readonly IBusBookingDbContext _context;

    public GetBusLocationsQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<IReadOnlyList<BusLocationDto>> Handle(GetBusLocationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusLocations
            .AsNoTracking()
            .Select(l => new BusLocationDto
            {
                LocationId = l.LocationId,
                LocationName = l.LocationName,
                Code = l.Code
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed record GetBusLocationByIdQuery(int Id) : IRequest<BusLocationDto?>;

public sealed class GetBusLocationByIdQueryHandler : IRequestHandler<GetBusLocationByIdQuery, BusLocationDto?>
{
    private readonly IBusBookingDbContext _context;

    public GetBusLocationByIdQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<BusLocationDto?> Handle(GetBusLocationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusLocations
            .AsNoTracking()
            .Where(l => l.LocationId == request.Id)
            .Select(l => new BusLocationDto
            {
                LocationId = l.LocationId,
                LocationName = l.LocationName,
                Code = l.Code
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed record CreateBusLocationCommand(BusLocationDto Location) : IRequest<BusLocationDto>;

public sealed class CreateBusLocationCommandHandler : IRequestHandler<CreateBusLocationCommand, BusLocationDto>
{
    private readonly IBusBookingDbContext _context;

    public CreateBusLocationCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<BusLocationDto> Handle(CreateBusLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = new BusLocation
        {
            LocationName = request.Location.LocationName,
            Code = request.Location.Code
        };

        _context.BusLocations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return request.Location with { LocationId = entity.LocationId };
    }
}

public sealed record UpdateBusLocationCommand(int Id, BusLocationDto Location) : IRequest<bool>;

public sealed class UpdateBusLocationCommandHandler : IRequestHandler<UpdateBusLocationCommand, bool>
{
    private readonly IBusBookingDbContext _context;

    public UpdateBusLocationCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateBusLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.BusLocations
            .FirstOrDefaultAsync(l => l.LocationId == request.Id, cancellationToken);

        if (location is null) return false;

        location.LocationName = request.Location.LocationName;
        location.Code = request.Location.Code;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record DeleteBusLocationCommand(int Id) : IRequest<bool>;

public sealed class DeleteBusLocationCommandHandler : IRequestHandler<DeleteBusLocationCommand, bool>
{
    private readonly IBusBookingDbContext _context;

    public DeleteBusLocationCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteBusLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.BusLocations
            .FirstOrDefaultAsync(l => l.LocationId == request.Id, cancellationToken);

        if (location is null) return false;

        _context.BusLocations.Remove(location);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record GetAddressByLocationIdQuery(int LocationId) : IRequest<IReadOnlyList<BusAddressLocationDto>>;

public sealed class GetAddressByLocationIdQueryHandler : IRequestHandler<GetAddressByLocationIdQuery, IReadOnlyList<BusAddressLocationDto>>
{
    private readonly IBusBookingDbContext _context;

    public GetAddressByLocationIdQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<IReadOnlyList<BusAddressLocationDto>> Handle(GetAddressByLocationIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusAddressLocations
            .AsNoTracking()
            .Where(al => al.LocationId == request.LocationId)
            .Select(al => new BusAddressLocationDto
            {
                LocationPointId = al.LocationPointId,
                LocationId = al.LocationId,
                LocationTitle = al.LocationTitle,
                LocationAddress = al.LocationAddress
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed record CreateBusAddressLocationCommand(BusAddressLocationDto AddressLocation) : IRequest<BusAddressLocationDto>;

public sealed class CreateBusAddressLocationCommandHandler : IRequestHandler<CreateBusAddressLocationCommand, BusAddressLocationDto>
{
    private readonly IBusBookingDbContext _context;

    public CreateBusAddressLocationCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<BusAddressLocationDto> Handle(CreateBusAddressLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = new BusAddressLocation
        {
            LocationId = request.AddressLocation.LocationId,
            LocationTitle = request.AddressLocation.LocationTitle,
            LocationAddress = request.AddressLocation.LocationAddress
        };

        _context.BusAddressLocations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return request.AddressLocation with { LocationPointId = entity.LocationPointId };
    }
}

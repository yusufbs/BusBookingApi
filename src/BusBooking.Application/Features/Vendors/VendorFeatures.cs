using BusBooking.Application.Common.Exceptions;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.DTOs;
using BusBooking.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Features.Vendors;

public sealed record GetBusVendorsQuery : IRequest<IReadOnlyList<BusVendorDto>>;

public sealed class GetBusVendorsQueryHandler : IRequestHandler<GetBusVendorsQuery, IReadOnlyList<BusVendorDto>>
{
    private readonly IBusBookingDbContext _context;

    public GetBusVendorsQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<IReadOnlyList<BusVendorDto>> Handle(GetBusVendorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusVendors
            .AsNoTracking()
            .Where(v => v.IsActive)
            .Select(v => new BusVendorDto
            {
                VendorId = v.VendorId,
                VendorName = v.VendorName,
                ContactNo = v.ContactNo,
                EmailId = v.EmailId,
                IsActive = v.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed record GetBusVendorByIdQuery(int Id) : IRequest<BusVendorDto?>;

public sealed class GetBusVendorByIdQueryHandler : IRequestHandler<GetBusVendorByIdQuery, BusVendorDto?>
{
    private readonly IBusBookingDbContext _context;

    public GetBusVendorByIdQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<BusVendorDto?> Handle(GetBusVendorByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusVendors
            .AsNoTracking()
            .Where(v => v.VendorId == request.Id)
            .Select(v => new BusVendorDto
            {
                VendorId = v.VendorId,
                VendorName = v.VendorName,
                ContactNo = v.ContactNo,
                EmailId = v.EmailId,
                IsActive = v.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed record CreateBusVendorCommand(BusVendorDto Vendor) : IRequest<BusVendorDto>;

public sealed class CreateBusVendorCommandHandler : IRequestHandler<CreateBusVendorCommand, BusVendorDto>
{
    private readonly IBusBookingDbContext _context;

    public CreateBusVendorCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<BusVendorDto> Handle(CreateBusVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = new BusVendor
        {
            VendorName = request.Vendor.VendorName,
            ContactNo = request.Vendor.ContactNo,
            EmailId = request.Vendor.EmailId,
            IsActive = request.Vendor.IsActive
        };

        _context.BusVendors.Add(vendor);
        await _context.SaveChangesAsync(cancellationToken);

        return request.Vendor with { VendorId = vendor.VendorId };
    }
}

public sealed record UpdateBusVendorCommand(int Id, BusVendorDto Vendor) : IRequest<bool>;

public sealed class UpdateBusVendorCommandHandler : IRequestHandler<UpdateBusVendorCommand, bool>
{
    private readonly IBusBookingDbContext _context;

    public UpdateBusVendorCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateBusVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.BusVendors
            .FirstOrDefaultAsync(v => v.VendorId == request.Id, cancellationToken);

        if (vendor is null) return false;

        vendor.VendorName = request.Vendor.VendorName;
        vendor.ContactNo = request.Vendor.ContactNo;
        vendor.EmailId = request.Vendor.EmailId;
        vendor.IsActive = request.Vendor.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record DeleteBusVendorCommand(int Id) : IRequest<bool>;

public sealed class DeleteBusVendorCommandHandler : IRequestHandler<DeleteBusVendorCommand, bool>
{
    private readonly IBusBookingDbContext _context;

    public DeleteBusVendorCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteBusVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _context.BusVendors
            .FirstOrDefaultAsync(v => v.VendorId == request.Id, cancellationToken);

        if (vendor is null) return false;

        // Soft delete
        vendor.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

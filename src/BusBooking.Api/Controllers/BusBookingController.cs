using Asp.Versioning;
using BusBooking.Application.Common.Models;
using BusBooking.Application.DTOs;
using BusBooking.Application.Features.Bookings;
using BusBooking.Application.Features.Bookings.Commands;
using BusBooking.Application.Features.Bookings.Queries;
using BusBooking.Application.Features.Locations;
using BusBooking.Application.Features.Schedules;
using BusBooking.Application.Features.Schedules.Queries;
using BusBooking.Application.Features.Vendors;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BusBooking.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("fixed-general")]
public sealed class BusBookingController : ControllerBase
{
    private readonly ISender _sender;

    public BusBookingController(ISender sender)
    {
        _sender = sender;
    }

    // ==========================================
    // 1. Bus Vendors
    // ==========================================

    [HttpGet("GetBusVendors")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager},{UserRole.Associate}")]
    [ProducesResponseType(typeof(IReadOnlyList<BusVendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBusVendors(CancellationToken ct)
    {
        var result = await _sender.Send(new GetBusVendorsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("GetBusVendorsById")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager},{UserRole.Associate}")]
    [ProducesResponseType(typeof(BusVendorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusVendorsById([FromQuery] int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetBusVendorByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("PostBusVendor")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(typeof(BusVendorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostBusVendor([FromBody] BusVendorDto model, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateBusVendorCommand(model), ct);
        return Ok(result);
    }

    [HttpPut("PutBusVendors")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutBusVendors([FromQuery] int id, [FromBody] BusVendorDto model, CancellationToken ct)
    {
        var success = await _sender.Send(new UpdateBusVendorCommand(id, model), ct);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("DeleteBusVendor")]
    [Authorize(Roles = UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBusVendor([FromQuery] int id, CancellationToken ct)
    {
        var success = await _sender.Send(new DeleteBusVendorCommand(id), ct);
        return success ? Ok() : NotFound();
    }

    // ==========================================
    // 3. Bus Locations & Boarding Points
    // ==========================================

    [HttpGet("GetBusLocations")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<BusLocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBusLocations(CancellationToken ct)
    {
        var result = await _sender.Send(new GetBusLocationsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("GetBusLocationById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BusLocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusLocationById([FromQuery] int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetBusLocationByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("PostBusLocation")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(typeof(BusLocationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostBusLocation([FromBody] BusLocationDto model, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateBusLocationCommand(model), ct);
        return Ok(result);
    }

    [HttpPut("PutBusLocation")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutBusLocation([FromQuery] int id, [FromBody] BusLocationDto model, CancellationToken ct)
    {
        var success = await _sender.Send(new UpdateBusLocationCommand(id, model), ct);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("DeleteBusLocation")]
    [Authorize(Roles = UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBusLocation([FromQuery] int id, CancellationToken ct)
    {
        var success = await _sender.Send(new DeleteBusLocationCommand(id), ct);
        return success ? Ok() : NotFound();
    }

    [HttpGet("getAddressByLocationId")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<BusAddressLocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddressByLocationId([FromQuery] int locationid, CancellationToken ct)
    {
        var result = await _sender.Send(new GetAddressByLocationIdQuery(locationid), ct);
        return Ok(result);
    }

    [HttpPost("PostBusLocationAddress")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(typeof(BusAddressLocationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostBusLocationAddress([FromBody] BusAddressLocationDto model, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateBusAddressLocationCommand(model), ct);
        return Ok(result);
    }

    // ==========================================
    // 4. Schedules, Routes & Search
    // ==========================================

    [HttpGet("GetAvailableRoutes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<AvailableRouteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableRoutes(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAvailableRoutesQuery(), ct);
        return Ok(result);
    }

    [HttpGet("GetBusSchedules")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager},{UserRole.Associate}")]
    [ProducesResponseType(typeof(IReadOnlyList<BusScheduleDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBusSchedules([FromQuery] int vendorId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetBusSchedulesByVendorQuery(vendorId), ct);
        return Ok(result);
    }

    [HttpGet("GetBusScheduleById")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager},{UserRole.Associate}")]
    [ProducesResponseType(typeof(BusScheduleDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusScheduleById([FromQuery] int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetBusScheduleByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("PostBusSchedule")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PostBusSchedule([FromBody] BusScheduleDetailsDto model, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateBusScheduleCommand(
            model.VendorId,
            model.BusName,
            model.BusVehicleNo,
            int.TryParse(model.FromLocationName, out var from) ? from : 1,
            int.TryParse(model.ToLocationName, out var to) ? to : 2,
            model.DepartureTime,
            model.ArrivalTime,
            model.ScheduleDate,
            model.Price,
            model.TotalSeats), ct);

        return Ok(model with { ScheduleId = id });
    }

    [HttpPut("PutBusSchedule")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutBusSchedule([FromQuery] int id, [FromBody] BusScheduleDetailsDto model, CancellationToken ct)
    {
        var success = await _sender.Send(new UpdateBusScheduleCommand(
            id,
            model.VendorId,
            model.BusName,
            model.BusVehicleNo,
            int.TryParse(model.FromLocationName, out var from) ? from : 1,
            int.TryParse(model.ToLocationName, out var to) ? to : 2,
            model.DepartureTime,
            model.ArrivalTime,
            model.ScheduleDate,
            model.Price,
            model.TotalSeats), ct);

        return success ? Ok() : NotFound();
    }

    [HttpDelete("DeleteBusSchedule")]
    [Authorize(Roles = UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBusSchedule([FromQuery] int id, CancellationToken ct)
    {
        var success = await _sender.Send(new DeleteBusScheduleCommand(id), ct);
        return success ? Ok() : NotFound();
    }

    [HttpGet("searchBus")]
    [AllowAnonymous]
    [EnableRateLimiting("sliding-search")]
    [ProducesResponseType(typeof(IReadOnlyList<BusScheduleDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchBus(
        [FromQuery] int fromLocation, 
        [FromQuery] int toLocation, 
        [FromQuery] string travelDate, 
        CancellationToken ct)
    {
        var result = await _sender.Send(new SearchBusQuery(fromLocation, toLocation, travelDate), ct);
        return Ok(result);
    }

    [HttpGet("searchBus2")]
    [AllowAnonymous]
    [EnableRateLimiting("sliding-search")]
    [ProducesResponseType(typeof(IReadOnlyList<BusScheduleDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchBus2(
        [FromQuery] int fromLocation, 
        [FromQuery] int toLocation, 
        [FromQuery] string travelDate, 
        CancellationToken ct)
    {
        var result = await _sender.Send(new SearchBusQuery(fromLocation, toLocation, travelDate), ct);
        return Ok(result);
    }

    // ==========================================
    // 5. Bookings & Seat Allocation
    // ==========================================

    [HttpGet("getBookedSeats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookedSeats([FromQuery] int shceduleId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetBookedSeatsQuery(shceduleId), ct);
        return Ok(result);
    }

    [HttpGet("GetAllBusBookings")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager},{UserRole.Associate}")]
    [ProducesResponseType(typeof(IReadOnlyList<BusBookingListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBusBookings([FromQuery] int? vendorId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetAllBusBookingsQuery(vendorId), ct);
        return Ok(result);
    }

    [HttpGet("GetBusBooking")]
    [Authorize]
    [ProducesResponseType(typeof(BusBookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusBooking([FromQuery] int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetBusBookingByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("PostBusBooking")]
    [Authorize]
    [EnableRateLimiting("token-booking")]
    [ProducesResponseType(typeof(BusBookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostBusBooking([FromBody] BusBookingViewModel model, CancellationToken ct)
    {
        var command = new CreateBusBookingCommand(
            model.CustId,
            model.ScheduleId,
            model.BusBookingPassengers);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("DeleteBusBooking")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBusBooking([FromQuery] int id, CancellationToken ct)
    {
        var success = await _sender.Send(new DeleteBusBookingCommand(id), ct);
        return success ? Ok() : NotFound();
    }
}

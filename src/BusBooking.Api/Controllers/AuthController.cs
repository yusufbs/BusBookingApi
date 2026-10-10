using Asp.Versioning;
using BusBooking.Application.Common.Models;
using BusBooking.Application.DTOs;
using BusBooking.Application.Features.Users;
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
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieve all registered users. Restricted to Admin and Manager roles.
    /// </summary>
    [HttpGet("GetAllUsers")]
    [Authorize(Roles = $"{UserRole.Admin},{UserRole.Manager}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var response = await _sender.Send(new GetAllUsersQuery(), ct);
        return Ok(response);
    }

    /// <summary>
    /// Authenticate credentials and return a signed JWT token along with user details.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginUserDto model, CancellationToken ct)
    {
        var response = await _sender.Send(new LoginUserCommand(model.UserName, model.Password), ct);
        return Ok(response);
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("AddNewUser")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddNewUser([FromBody] UserDto model, CancellationToken ct)
    {
        var response = await _sender.Send(new AddNewUserCommand(model), ct);
        return Ok(response);
    }

    /// <summary>
    /// Update existing user account details. Requires authorization.
    /// </summary>
    [HttpPost("UpdateUser")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUser([FromBody] UserDto model, CancellationToken ct)
    {
        var response = await _sender.Send(new UpdateUserCommand(model), ct);
        return Ok(response);
    }

    /// <summary>
    /// Delete user account by UserId. Restricted to Admin role.
    /// </summary>
    [HttpDelete("DeleteUserByUserId")]
    [Authorize(Roles = UserRole.Admin)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteUserByUserId([FromQuery] int userId, CancellationToken ct)
    {
        var response = await _sender.Send(new DeleteUserCommand(userId), ct);
        return Ok(response);
    }
}

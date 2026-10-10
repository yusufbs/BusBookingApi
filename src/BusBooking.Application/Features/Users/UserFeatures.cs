using BusBooking.Application.Common.Exceptions;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.Common.Models;
using BusBooking.Application.DTOs;
using BusBooking.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Features.Users;

public sealed record GetAllUsersQuery : IRequest<ApiResponse<IReadOnlyList<UserDto>>>;

public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ApiResponse<IReadOnlyList<UserDto>>>
{
    private readonly IBusBookingDbContext _context;

    public GetAllUsersQueryHandler(IBusBookingDbContext context) => _context = context;

    public async Task<ApiResponse<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                EmailId = u.EmailId,
                FullName = u.FullName,
                Role = u.Role,
                Password = "●●●●●●●●",
                CreatedDate = u.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                RefreshToken = u.RefreshToken,
                RefreshTokenExpiryTime = u.RefreshTokenExpiryTime.HasValue ? u.RefreshTokenExpiryTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<UserDto>>.Success(users, "Users retrieved successfully.");
    }
}

public sealed record LoginUserCommand(string UserName, string Password) : IRequest<ApiResponse<UserDto>>;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ApiResponse<UserDto>>
{
    private readonly IBusBookingDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginUserCommandHandler(IBusBookingDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ApiResponse<UserDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
        {
            return ApiResponse<UserDto>.Failure("Invalid username or password.");
        }

        var token = _jwtTokenService.GenerateToken(user.UserId, user.UserName, user.EmailId, user.Role);
        var refreshToken = Guid.NewGuid().ToString("N");
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new UserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            EmailId = user.EmailId,
            FullName = user.FullName,
            Role = user.Role,
            Password = string.Empty,
            CreatedDate = user.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = user.RefreshTokenExpiryTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        return ApiResponse<UserDto>.Success(dto, "Login successful.");
    }
}

public sealed record AddNewUserCommand(UserDto User) : IRequest<ApiResponse<UserDto>>;

public sealed class AddNewUserCommandHandler : IRequestHandler<AddNewUserCommand, ApiResponse<UserDto>>
{
    private readonly IBusBookingDbContext _context;

    public AddNewUserCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<ApiResponse<UserDto>> Handle(AddNewUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.Users
            .AnyAsync(u => u.UserName == request.User.UserName || u.EmailId == request.User.EmailId, cancellationToken);

        if (existing)
        {
            return ApiResponse<UserDto>.Failure("A user with this username or email already exists.");
        }

        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.User.Password, 12);

        // Normalize role to Admin, Manager, Associate, or Customer
        var role = request.User.Role?.Trim();
        if (string.IsNullOrWhiteSpace(role) || !Domain.Enums.UserRole.AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            role = Domain.Enums.UserRole.Associate;
        }

        var user = new User
        {
            UserName = request.User.UserName,
            EmailId = request.User.EmailId,
            FullName = request.User.FullName,
            Role = role,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var result = request.User with
        {
            UserId = user.UserId,
            Role = user.Role,
            Password = string.Empty,
            CreatedDate = user.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        return ApiResponse<UserDto>.Success(result, "User registered successfully.");
    }
}

public sealed record UpdateUserCommand(UserDto User) : IRequest<ApiResponse<UserDto>>;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    private readonly IBusBookingDbContext _context;

    public UpdateUserCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == request.User.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UserDto>.Failure("User not found.");
        }

        user.FullName = request.User.FullName;
        user.EmailId = request.User.EmailId;
        user.Role = request.User.Role;

        if (!string.IsNullOrWhiteSpace(request.User.Password) && request.User.Password != "●●●●●●●●")
        {
            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.User.Password, 12);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<UserDto>.Success(request.User, "User updated successfully.");
    }
}

public sealed record DeleteUserCommand(int UserId) : IRequest<ApiResponse>;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse>
{
    private readonly IBusBookingDbContext _context;

    public DeleteUserCommandHandler(IBusBookingDbContext context) => _context = context;

    public async Task<ApiResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse.Failure("User not found.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(null, "User deleted successfully.");
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Dtos.Auth;
using UHPS.API.Entities;
using UHPS.API.Options;

namespace UHPS.API.Services;

public class AuthService : IAuthService
{
    private const int CustomerRoleId = 4;

    private readonly AppDbContext _db;
    private readonly JwtSettings _jwtSettings;
    private readonly ICurrentUser _currentUser;

    public AuthService(AppDbContext db, IOptions<JwtSettings> jwtOptions, ICurrentUser currentUser)
    {
        _db = db;
        _jwtSettings = jwtOptions.Value;
        _currentUser = currentUser;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (emailExists) return null;

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            RoleId = CustomerRoleId
        };

        var customer = new Customer
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            User = user
        };

        _db.Users.Add(user);
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        var role = await _db.Roles.FirstAsync(r => r.Id == CustomerRoleId, ct);
        return BuildResponse(user, role);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

        return BuildResponse(user, user.Role);
    }

    public async Task<UserResponse?> GetCurrentUserAsync(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, ct);

        if (user is null) return null;

        return new UserResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.Name
        };
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return false;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, ct);
        if (user is null) return false;

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ValidationException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 11);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private AuthResponse BuildResponse(User user, Role role)
    {
        var (token, expiresAt) = GenerateToken(user, role);
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email,
            Role = role.Name
        };
    }

    private (string token, DateTime expiresAt) GenerateToken(User user, Role role)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

using UHPS.API.Dtos.Auth;

namespace UHPS.API.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<UserResponse?> GetCurrentUserAsync(CancellationToken ct);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct);
}

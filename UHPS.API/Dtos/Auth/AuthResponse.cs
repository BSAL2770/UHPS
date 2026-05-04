namespace UHPS.API.Dtos.Auth;

public class AuthResponse
{
    public required string Token { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required int UserId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}

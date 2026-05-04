namespace UHPS.API.Dtos.Auth;

public class UserResponse
{
    public required int UserId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}

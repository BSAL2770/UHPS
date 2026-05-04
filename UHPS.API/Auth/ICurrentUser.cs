namespace UHPS.API.Auth;

public interface ICurrentUser
{
    int? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(params string[] roles);
}

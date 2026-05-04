using System.ComponentModel.DataAnnotations;

namespace UHPS.API.Dtos.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }
}

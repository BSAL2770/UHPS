using System.ComponentModel.DataAnnotations;
using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Employees;

public class EmployeeCreateRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; set; }

    [Required]
    [Phone]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }

    [Required]
    public required AddressRequest Address { get; set; }

    public int? StoreId { get; set; }

    [Required]
    [RegularExpression("^(Employee|Supervisor)$",
        ErrorMessage = "Role must be 'Employee' or 'Supervisor'.")]
    public required string Role { get; set; }
}

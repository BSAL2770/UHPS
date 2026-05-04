using System.ComponentModel.DataAnnotations;
using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Employees;

public class EmployeeUpdateRequest
{
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
}

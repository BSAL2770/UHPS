using System.ComponentModel.DataAnnotations;
using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Customers;

public class CustomerUpdateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public AddressRequest? Address { get; set; }
}

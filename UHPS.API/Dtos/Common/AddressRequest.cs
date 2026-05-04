using System.ComponentModel.DataAnnotations;

namespace UHPS.API.Dtos.Common;

public class AddressRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string StreetAddress { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string City { get; set; }

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public required string State { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 5)]
    public required string Zipcode { get; set; }
}

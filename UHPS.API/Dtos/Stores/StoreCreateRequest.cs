using System.ComponentModel.DataAnnotations;
using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Stores;

public class StoreCreateRequest
{
    [Required]
    [Phone]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }

    public AddressRequest? Address { get; set; }

    public int? SupervisorId { get; set; }
}

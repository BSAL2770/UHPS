using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Customers;

public class CustomerResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public AddressResponse? Address { get; set; }
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

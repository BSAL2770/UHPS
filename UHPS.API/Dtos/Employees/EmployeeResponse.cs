using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Employees;

public class EmployeeResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }
    public required AddressResponse Address { get; set; }
    public int? StoreId { get; set; }
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

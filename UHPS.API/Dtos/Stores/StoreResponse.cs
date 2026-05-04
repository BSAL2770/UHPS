using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Stores;

public class StoreResponse
{
    public required int Id { get; set; }
    public required string PhoneNumber { get; set; }
    public AddressResponse? Address { get; set; }
    public int? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

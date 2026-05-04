using UHPS.API.Dtos.Common;
using UHPS.API.Entities;

namespace UHPS.API.Dtos.Packages;

public class PackageResponse
{
    public required int Id { get; set; }
    public int? SenderId { get; set; }
    public string? SenderName { get; set; }
    public int? ReceiverId { get; set; }
    public string? ReceiverName { get; set; }
    public required AddressResponse Address { get; set; }
    public string? Description { get; set; }
    public required PackageStatus Status { get; set; }
    public required decimal Weight { get; set; }
    public required decimal Width { get; set; }
    public required decimal Height { get; set; }
    public required decimal Depth { get; set; }
    public required bool Express { get; set; }
    public required int ShipmentId { get; set; }
    public string? ShipmentDescription { get; set; }
    public decimal? ShipCost { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

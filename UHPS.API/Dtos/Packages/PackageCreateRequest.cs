using System.ComponentModel.DataAnnotations;
using UHPS.API.Dtos.Common;

namespace UHPS.API.Dtos.Packages;

public class PackageCreateRequest
{
    public int? SenderId { get; set; }
    public int? ReceiverId { get; set; }

    [Required]
    public required AddressRequest Address { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Weight { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Width { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Height { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Depth { get; set; }

    public bool Express { get; set; }

    [Range(1, int.MaxValue)]
    public int ShipmentId { get; set; }
}

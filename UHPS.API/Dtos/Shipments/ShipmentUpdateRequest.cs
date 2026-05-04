using System.ComponentModel.DataAnnotations;

namespace UHPS.API.Dtos.Shipments;

public class ShipmentUpdateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Description { get; set; }

    [Range(0.01, 9999.99)]
    public decimal MaxLength { get; set; }

    [Range(0.01, 9999.99)]
    public decimal MaxWidth { get; set; }

    [Range(0.01, 9999.99)]
    public decimal MaxHeight { get; set; }

    [Range(0.00, 9_999_999_999_999.99)]
    public decimal GroundCost { get; set; }

    [Range(0.00, 9_999_999_999_999.99)]
    public decimal ExpressCost { get; set; }
}

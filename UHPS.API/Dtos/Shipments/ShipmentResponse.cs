namespace UHPS.API.Dtos.Shipments;

public class ShipmentResponse
{
    public required int Id { get; set; }
    public required string Description { get; set; }
    public required decimal MaxLength { get; set; }
    public required decimal MaxWidth { get; set; }
    public required decimal MaxHeight { get; set; }
    public required decimal GroundCost { get; set; }
    public required decimal ExpressCost { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

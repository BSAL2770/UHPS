namespace UHPS.API.Entities;

public class Shipment : BaseEntity
{
    public required string Description { get; set; }
    public decimal MaxLength { get; set; }
    public decimal MaxWidth { get; set; }
    public decimal MaxHeight { get; set; }
    public decimal GroundCost { get; set; }
    public decimal ExpressCost { get; set; }

    public ICollection<Package> Packages { get; set; } = new List<Package>();
}

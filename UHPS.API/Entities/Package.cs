namespace UHPS.API.Entities;

public class Package : BaseEntity
{
    public int? SenderId { get; set; }
    public Customer? Sender { get; set; }

    public int? ReceiverId { get; set; }
    public Customer? Receiver { get; set; }

    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;

    public string? Description { get; set; }
    public PackageStatus Status { get; set; }

    public decimal Weight { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Depth { get; set; }

    public bool Express { get; set; }

    public int ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public decimal? ShipCost { get; set; }

    public ICollection<TrackingRecord> TrackingRecords { get; set; } = new List<TrackingRecord>();
}

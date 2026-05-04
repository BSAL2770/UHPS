namespace UHPS.API.Entities;

public class TrackingRecord : BaseEntity
{
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int PackageId { get; set; }
    public Package Package { get; set; } = null!;

    public int StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public DateTime? TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }

    public int? DestinationAddressId { get; set; }
    public Address? DestinationAddress { get; set; }
}

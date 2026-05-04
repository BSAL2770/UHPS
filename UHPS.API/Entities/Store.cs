namespace UHPS.API.Entities;

public class Store : BaseEntity
{
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }

    public int? SupervisorId { get; set; }
    public Employee? Supervisor { get; set; }

    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<TrackingRecord> TrackingRecords { get; set; } = new List<TrackingRecord>();
}

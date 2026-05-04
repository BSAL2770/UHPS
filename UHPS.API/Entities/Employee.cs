namespace UHPS.API.Entities;

public class Employee : BaseEntity
{
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }

    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;

    public int? StoreId { get; set; }
    public Store? Store { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Store> SupervisedStores { get; set; } = new List<Store>();
    public ICollection<TrackingRecord> TrackingRecords { get; set; } = new List<TrackingRecord>();
}

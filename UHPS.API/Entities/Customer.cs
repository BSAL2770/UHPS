namespace UHPS.API.Entities;

public class Customer : BaseEntity
{
    public required string Name { get; set; }
    public string? PhoneNumber { get; set; }

    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Package> SentPackages { get; set; } = new List<Package>();
    public ICollection<Package> ReceivedPackages { get; set; } = new List<Package>();
}

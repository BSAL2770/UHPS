namespace UHPS.API.Entities;

public class Address : BaseEntity
{
    public required string StreetAddress { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string Zipcode { get; set; }
}

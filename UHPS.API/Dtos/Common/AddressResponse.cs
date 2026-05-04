namespace UHPS.API.Dtos.Common;

public class AddressResponse
{
    public required int Id { get; set; }
    public required string StreetAddress { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string Zipcode { get; set; }
}

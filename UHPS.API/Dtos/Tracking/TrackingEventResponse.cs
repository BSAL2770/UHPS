using UHPS.API.Entities;

namespace UHPS.API.Dtos.Tracking;

public class TrackingEventResponse
{
    public required int Id { get; set; }
    public required DateTime ScannedAt { get; set; }
    public required PackageStatus Status { get; set; }
    public required int StoreId { get; set; }
    public required string StoreName { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Notes { get; set; }
}

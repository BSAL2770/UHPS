using UHPS.API.Entities;

namespace UHPS.API.Dtos.Tracking;

// Public-facing reduced view of a tracking event. Deliberately omits EmployeeId/EmployeeName/Notes —
// those leak internal staff identity and free-text that may include sensitive info.
public class PublicTrackingEventResponse
{
    public required DateTime ScannedAt { get; set; }
    public required PackageStatus Status { get; set; }
    public required string StoreName { get; set; }
}

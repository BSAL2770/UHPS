using UHPS.API.Entities;

namespace UHPS.API.Dtos.Tracking;

// Reduced public view: status + last 5 events. No sender/receiver, no cost, no notes,
// no employee identity. Safe to expose anonymously.
public class PublicTrackingResponse
{
    public required int PackageId { get; set; }
    public required PackageStatus CurrentStatus { get; set; }
    public required IReadOnlyList<PublicTrackingEventResponse> RecentEvents { get; set; }
}

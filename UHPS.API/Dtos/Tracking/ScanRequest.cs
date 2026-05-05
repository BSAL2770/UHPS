using System.ComponentModel.DataAnnotations;
using UHPS.API.Entities;

namespace UHPS.API.Dtos.Tracking;

public class ScanRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int StoreId { get; set; }

    public PackageStatus? NewStatus { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

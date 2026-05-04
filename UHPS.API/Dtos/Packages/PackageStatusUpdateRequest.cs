using System.ComponentModel.DataAnnotations;
using UHPS.API.Entities;

namespace UHPS.API.Dtos.Packages;

public class PackageStatusUpdateRequest
{
    [Required]
    public required PackageStatus Status { get; set; }
}

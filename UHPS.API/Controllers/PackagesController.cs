using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Dtos.Packages;
using UHPS.API.Dtos.Tracking;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

[ApiController]
[Route("api/packages")]
[Authorize]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packages;
    private readonly ITrackingService _tracking;

    public PackagesController(IPackageService packages, ITrackingService tracking)
    {
        _packages = packages;
        _tracking = tracking;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(PagedResult<PackageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PackageResponse>>> GetAll(
        [FromQuery] PagingQuery paging,
        CancellationToken ct)
    {
        var result = await _packages.GetAllAsync(paging, ct);
        return Ok(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(PagedResult<PackageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PackageResponse>>> GetMyPackages(
        [FromQuery] PagingQuery paging,
        CancellationToken ct)
    {
        var result = await _packages.GetCurrentCustomerPackagesAsync(paging, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PackageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageResponse>> GetById(int id, CancellationToken ct)
    {
        var package = await _packages.GetByIdAsync(id, ct);
        return package is null ? NotFound() : Ok(package);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor},{Roles.Customer}")]
    [ProducesResponseType(typeof(PackageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PackageResponse>> Create(
        [FromBody] PackageCreateRequest request,
        CancellationToken ct)
    {
        var created = await _packages.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrSupervisor)]
    [ProducesResponseType(typeof(PackageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageResponse>> Update(
        int id,
        [FromBody] PackageUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _packages.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(PackageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageResponse>> UpdateStatus(
        int id,
        [FromBody] PackageStatusUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _packages.UpdateStatusAsync(id, request.Status, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpGet("{id:int}/tracking")]
    [ProducesResponseType(typeof(PackageTrackingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageTrackingResponse>> GetTracking(int id, CancellationToken ct)
    {
        var history = await _tracking.GetPackageHistoryAsync(id, ct);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpPost("{id:int}/scan")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(TrackingEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackingEventResponse>> Scan(
        int id,
        [FromBody] ScanRequest request,
        CancellationToken ct)
    {
        var record = await _tracking.ScanAsync(id, request, ct);
        return record is null
            ? NotFound()
            : StatusCode(StatusCodes.Status201Created, record);
    }
}

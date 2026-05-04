using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Dtos.Packages;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

[ApiController]
[Route("api/packages")]
[Authorize]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packages;

    public PackagesController(IPackageService packages)
    {
        _packages = packages;
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
    [Authorize(Roles = Roles.Customer)]
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
}

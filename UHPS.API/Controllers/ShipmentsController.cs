using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Dtos.Shipments;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipments;

    public ShipmentsController(IShipmentService shipments)
    {
        _shipments = shipments;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ShipmentResponse>>> GetAll(
        [FromQuery] PagingQuery paging,
        CancellationToken ct)
    {
        var result = await _shipments.GetAllAsync(paging, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentResponse>> GetById(int id, CancellationToken ct)
    {
        var shipment = await _shipments.GetByIdAsync(id, ct);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ShipmentResponse>> Create(
        [FromBody] ShipmentCreateRequest request,
        CancellationToken ct)
    {
        var created = await _shipments.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentResponse>> Update(
        int id,
        [FromBody] ShipmentUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _shipments.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _shipments.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}

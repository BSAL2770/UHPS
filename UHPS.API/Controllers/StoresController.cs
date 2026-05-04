using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Dtos.Stores;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

[ApiController]
[Route("api/stores")]
[Authorize]
public class StoresController : ControllerBase
{
    private readonly IStoreService _stores;

    public StoresController(IStoreService stores)
    {
        _stores = stores;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StoreResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StoreResponse>>> GetAll(
        [FromQuery] PagingQuery paging,
        CancellationToken ct)
    {
        var result = await _stores.GetAllAsync(paging, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreResponse>> GetById(int id, CancellationToken ct)
    {
        var store = await _stores.GetByIdAsync(id, ct);
        return store is null ? NotFound() : Ok(store);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StoreResponse>> Create(
        [FromBody] StoreCreateRequest request,
        CancellationToken ct)
    {
        var created = await _stores.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreResponse>> Update(
        int id,
        [FromBody] StoreUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _stores.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _stores.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}

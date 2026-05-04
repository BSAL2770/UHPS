using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Dtos.Customers;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;

    public CustomersController(ICustomerService customers)
    {
        _customers = customers;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(PagedResult<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CustomerResponse>>> GetAll(
        [FromQuery] PagingQuery paging,
        CancellationToken ct)
    {
        var result = await _customers.GetAllAsync(paging, ct);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetMe(CancellationToken ct)
    {
        var customer = await _customers.GetCurrentAsync(ct);
        return customer is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "No customer profile",
                Detail = "No customer profile is linked to this user."
            })
            : Ok(customer);
    }

    [HttpPut("me")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> UpdateMe(
        [FromBody] CustomerUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _customers.UpdateCurrentAsync(request, ct);
        return updated is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "No customer profile",
                Detail = "No customer profile is linked to this user."
            })
            : Ok(updated);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(id, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> Update(
        int id,
        [FromBody] CustomerUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _customers.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _customers.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}

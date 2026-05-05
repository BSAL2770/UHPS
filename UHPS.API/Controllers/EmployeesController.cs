using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Dtos.Employees;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private const string EmployeeOrSupervisor = $"{Roles.Employee},{Roles.Supervisor}";

    private readonly IEmployeeService _employees;

    public EmployeesController(IEmployeeService employees)
    {
        _employees = employees;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSupervisor)]
    [ProducesResponseType(typeof(PagedResult<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetAll(
        [FromQuery] PagingQuery paging,
        CancellationToken ct)
    {
        var result = await _employees.GetAllAsync(paging, ct);
        return Ok(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetMe(CancellationToken ct)
    {
        var employee = await _employees.GetCurrentAsync(ct);
        return employee is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "No employee profile",
                Detail = "No employee profile is linked to this user."
            })
            : Ok(employee);
    }

    [HttpPut("me")]
    [Authorize(Roles = EmployeeOrSupervisor)]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> UpdateMe(
        [FromBody] EmployeeSelfUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _employees.UpdateCurrentAsync(request, ct);
        return updated is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "No employee profile",
                Detail = "No employee profile is linked to this user."
            })
            : Ok(updated);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken ct)
    {
        var employee = await _employees.GetByIdAsync(id, ct);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeResponse>> Create(
        [FromBody] EmployeeCreateRequest request,
        CancellationToken ct)
    {
        var created = await _employees.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrSupervisor)]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        int id,
        [FromBody] EmployeeUpdateRequest request,
        CancellationToken ct)
    {
        var updated = await _employees.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _employees.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}

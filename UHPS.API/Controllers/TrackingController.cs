using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UHPS.API.Dtos.Tracking;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

// Public-facing tracking endpoint. Anonymous lookup by package id, reduced data shape.
//
// SECURITY NOTE — production deployments must add per-IP rate limiting and ideally
// non-sequential tracking numbers. With sequential ids and no rate limit, an attacker can
// enumerate the entire system. Out of scope for the 7-day portfolio build; documented in README.
[ApiController]
[Route("api/tracking")]
[AllowAnonymous]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _tracking;

    public TrackingController(ITrackingService tracking)
    {
        _tracking = tracking;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PublicTrackingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicTrackingResponse>> GetPublicTracking(int id, CancellationToken ct)
    {
        var result = await _tracking.GetPublicTrackingAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

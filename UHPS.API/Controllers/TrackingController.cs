using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UHPS.API.Dtos.Tracking;
using UHPS.API.Services;

namespace UHPS.API.Controllers;

// Public-facing tracking endpoint. Anonymous lookup by package id, reduced data shape.
// Rate limited per-IP at 60 req/min via the "public-tracking" policy in Program.cs.
// Distributed enumeration (botnets) defeats this; the proper fix is non-sequential
// public tracking numbers, documented in the README under Known limitations.
[ApiController]
[Route("api/tracking")]
[AllowAnonymous]
[EnableRateLimiting("public-tracking")]
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
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PublicTrackingResponse>> GetPublicTracking(int id, CancellationToken ct)
    {
        var result = await _tracking.GetPublicTrackingAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

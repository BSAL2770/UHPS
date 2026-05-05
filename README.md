# UHPS — UH Postal Service API

> Day 7 will expand this README. The sections below capture the production-readiness
> caveats that are deliberately out of scope for the 7-day portfolio build but matter
> in any real deployment.

## Known production-readiness gaps

### 1. Anonymous tracking endpoint has no rate limiting

`GET /api/tracking/{id}` is intentionally `[AllowAnonymous]` so the public can paste
a tracking number into a public page (matching USPS / UPS / FedEx behavior). It
returns a deliberately reduced shape — only status, last 5 events with timestamp,
status, and store name. No sender, receiver, addresses, costs, employee identity, or
notes.

**The risk:** sequential integer IDs combined with no rate limit means an attacker
can iterate `GET /api/tracking/1`, `/2`, `/3`, ... and enumerate every package in
the system. Even with the reduced shape, that leaks "this destination has shipments
moving through it" patterns.

**The production fix is two-pronged:**

1. **Per-IP rate limit on the public endpoint** (e.g., 60 req/min). ASP.NET Core
   ships `Microsoft.AspNetCore.RateLimiting` with `AddRateLimiter` — fixed-window or
   sliding-window policies are one-liners.
2. **Non-sequential tracking numbers.** Either GUIDs, or a short random alphanumeric
   public ID separate from the integer primary key. With 1B+ possible values,
   enumeration becomes economically infeasible.

Neither is implemented in this build. Both are 1-day adds when the project moves
beyond portfolio-defense scope.

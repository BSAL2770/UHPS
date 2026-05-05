# UHPS — UH Postal Service API

A package tracking and shipping management API. ASP.NET Core 9 Web API on PostgreSQL with JWT-authenticated role-based access (Admin, Supervisor, Employee, Customer) and an anonymous public tracking endpoint.

## About this project

UHPS started as a group project for a database systems course at the University of Houston in early 2022 — a Razor Pages monolith on SQL Server, built collaboratively by a 5-person team (Bader Salem, Son La, Morrison Fowlks, Andy Nguyen, Josh Opie) over the Jan–Apr 2022 semester to model a postal service. The application source was lost over time; only the SQL dump survived in the [original repo](https://github.com/BSAL2770/UHPS).

## Tech stack

- ASP.NET Core 9 Web API (controller-based)
- Entity Framework Core 9
- PostgreSQL 16 (Docker for local development)
- BCrypt for password hashing
- JWT bearer tokens for authentication
- Swashbuckle / Swagger for OpenAPI
- xUnit-friendly architecture (services are interface-injected; not yet covered by tests in this build)

## Setup

Assumes .NET 9 SDK and Docker Desktop installed.

```bash
# 1. Clone, restore, build
git clone https://github.com/BSAL2770/UHPS.git
cd UHPS
dotnet restore
dotnet build

# 2. Bring up Postgres
docker compose up -d

# 3. Configure the JWT signing key (one-time, per machine)
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)" --project UHPS.API

# 4. Apply migrations
dotnet ef database update --project UHPS.API

# 5. Run
dotnet run --project UHPS.API --launch-profile http
```

Swagger UI: http://localhost:5207/swagger

### A note on the dev Postgres password

`appsettings.json` ships with `Password=dev_password` for local development. This is intentional — the password lives in source so the project runs out of the box after a fresh clone. The Docker container only listens on `127.0.0.1:5432`, so the credential cannot reach anything beyond your machine. Production deployments override the entire connection string via `ConnectionStrings__DefaultConnection` (or, on Railway/similar, via `DATABASE_URL` parsed at startup).

The JWT signing key is the only secret that's *not* in source — it's set per-machine via `dotnet user-secrets`. Production sets `Jwt__Key` as an environment variable.

### Seeded admin user (development only)

```
Email:    admin@uhps.local
Password: Admin@123!
```

Created by the `SeedAdminUser` migration via EF Core's `HasData` with a precomputed BCrypt hash. The hash is checked into source under a `// DEV ONLY` comment in `UserConfiguration.cs`. Production deployments would seed an initial admin via env-var-driven scripts and then either rotate the password or delete this seeded row.

## API surface

Method | Path | Auth | Description
--- | --- | --- | ---
POST | `/api/auth/register` | Anonymous | Create a Customer account (email + password + name); returns JWT
POST | `/api/auth/login` | Anonymous | Returns JWT for valid credentials; generic 401 otherwise
GET | `/api/auth/me` | Any | Identity from JWT: `{userId, email, role}`
POST | `/api/auth/change-password` | Any | Rotate own password; verifies current password
GET | `/api/customers` | Staff | Paged list
GET | `/api/customers/me` | Any | Own Customer profile, or 404 if not linked
PUT | `/api/customers/me` | Customer | Update own profile (Name, Phone, Address)
GET | `/api/customers/{id}` | Any | Service-level ownership check: Customer must own
PUT | `/api/customers/{id}` | Any | Service-level: Admin/Supervisor any, Customer own only
DELETE | `/api/customers/{id}` | Admin | Soft delete
GET | `/api/employees` | Admin/Supervisor | Paged list
GET | `/api/employees/me` | Any | Own Employee profile, or 404 if not linked
PUT | `/api/employees/me` | Employee/Supervisor | Update own profile (Name, Phone, Address)
GET | `/api/employees/{id}` | Staff | Service-level: Employee can read self only
POST | `/api/employees` | Admin | Creates User + Employee in one transaction; assigns Role
PUT | `/api/employees/{id}` | Admin/Supervisor | Full update (admin-only fields included)
DELETE | `/api/employees/{id}` | Admin | Soft delete
GET | `/api/stores` | Any | Paged list
GET | `/api/stores/{id}` | Any | Single
POST | `/api/stores` | Admin | Create
PUT | `/api/stores/{id}` | Admin | Full update
DELETE | `/api/stores/{id}` | Admin | Soft delete
GET | `/api/shipments` | Any | Paged list of pricing tiers
GET | `/api/shipments/{id}` | Any | Single tier
POST | `/api/shipments` | Admin | Create tier
PUT | `/api/shipments/{id}` | Admin | Full update
DELETE | `/api/shipments/{id}` | Admin | Soft delete
GET | `/api/packages` | Staff | Paged list
GET | `/api/packages/me` | Any | Customer's own packages (sender or receiver); empty for non-customers
GET | `/api/packages/{id}` | Any | Service-level: Customer must be sender or receiver
POST | `/api/packages` | Admin/Supervisor/Customer | Customer's senderId is server-set from JWT; status forced to Created; shipCost computed server-side from tier + Express flag
PUT | `/api/packages/{id}` | Admin/Supervisor | Full update (excludes status)
PATCH | `/api/packages/{id}/status` | Staff | Status transition; Lost requires Admin/Supervisor
GET | `/api/packages/{id}/tracking` | Any | Full tracking history; Customer must own; descending order
POST | `/api/packages/{id}/scan` | Staff | Logs a TrackingRecord; optional newStatus updates Package.Status atomically; Lost transition requires Admin/Supervisor
GET | `/api/tracking/{id}` | Anonymous | Public reduced view: status + last 5 events (timestamp, status, store name only)

The Swagger UI at `/swagger` is the live spec — this table is for reference and may drift between releases.

## Architecture decisions

Numbered list of every non-obvious choice and the rationale. The structure for each is **What** (the choice), **Why** (the reasoning), **Trade-off** (what we gave up).

### 1. `decimal(18,2)` for money, never `float`/`real`

**What.** All currency fields (`ShipCost`, `GroundCost`, `ExpressCost`) are `decimal(18,2)` in the schema and `decimal` in C#. The original SQL dump used `real` (32-bit float).

**Why.** Floating-point types accumulate rounding errors on aggregation. `SUM(ShipCost)` over a thousand packages with `real` will return a slightly wrong number; with `decimal` it's exact. This is the textbook "money in float" anti-pattern.

**Trade-off.** Slightly slower arithmetic and ~16 bytes per value vs. 4. Worth it for any system that touches money.

### 2. Manual DTO mapping in services, not AutoMapper

**What.** Every `Service.MapToResponse(entity)` is a hand-written `new Response { Field = entity.Field, ... }` block in the service.

**Why.** AutoMapper introduces implicit mapping rules that surprise readers and Profile classes that drift from reality. For 5-7 entities the savings don't justify the dependency. Writing the mapping out makes it instantly obvious which fields are exposed and which are filtered.

**Trade-off.** ~15-25 lines of boilerplate per response DTO. At 50+ entities this would flip toward AutoMapper; at this scale, manual wins.

### 3. Inline ownership checks via a guard helper, not `IAuthorizationHandler`

**What.** Endpoints that combine "any role can call" + "but only on resources you own" use `AuthorizationGuards.EnsureOwnerOrInRole(currentUser, resource.UserId, ...privilegedRoles)` inside the service method. The full `IAuthorizationHandler` + `requirements` + `[Authorize(Policy=...)]` machinery is not used.

**Why.** Authorization handlers are the canonical .NET pattern for complex policies and they're worth knowing — but at 5 entities the framework overhead exceeds the value. Inline guard calls are easy to read in service code, easy to test, and put the check next to the data fetch where reviewers expect it.

**Trade-off.** Doesn't scale to dozens of resource types or to complex multi-attribute policies. The trade-off is consciously made; the talk track for any interview reviewer who asks is "handlers are the right pattern at scale, here's why scale isn't here yet."

### 4. Soft delete via EF Core global query filters

**What.** `BaseEntity` has a `Deleted bool` column and `BaseEntityConfiguration.ConfigureBase` calls `builder.HasQueryFilter(e => !e.Deleted)`. Every `DbContext.X.ToListAsync()` automatically excludes soft-deleted rows. Hard deletes are not used anywhere in the API.

**Why.** Many domains need an audit trail of "what used to exist." Hard delete loses information; soft delete preserves it without polluting service code with `.Where(x => !x.Deleted)` everywhere.

**Trade-off.** Bypassing the filter (for an admin "trash bin" view) requires `.IgnoreQueryFilters()`. The orphan-row problem when deleting a parent that owns children is real and not solved here — see Known limitations.

### 5. Fluent API for entity configuration, DataAnnotations for request DTOs

**What.** Persistence concerns (column types, FKs, indexes, query filters) live in `IEntityTypeConfiguration<T>` classes under `Data/Configurations/`. Validation concerns (`[Required]`, `[StringLength]`, `[EmailAddress]`) live as DataAnnotations on the request DTOs.

**Why.** They're different concerns and they read different code paths. Fluent API on entities keeps the entities clean POCOs (no `[Column]` attributes leaking persistence into the domain). DataAnnotations on DTOs work seamlessly with ASP.NET Core's model binding to give 400 BadRequest with field-level error messages for free.

**Trade-off.** Two patterns instead of one — slight cognitive overhead. The boundary is clear though: entity configuration is in `Data/Configurations/`, DTO validation is in `Dtos/`.

### 6. Status state machine with dedicated PATCH endpoint

**What.** `Package.Status` is not writable through the general `PUT /api/packages/{id}` endpoint. Status transitions go through `PATCH /api/packages/{id}/status` (admin correction) or `POST /api/packages/{id}/scan` (real-world event).

**Why.** Status is special — it's the field most likely to be subject to business rules (Lost requires Admin/Supervisor; eventually a state-machine constraint like "can't go from Delivered back to InTransit"). Putting it on the general PUT means every PUT call has to remember to preserve status correctly. A dedicated endpoint scoped to status enforces a single code path.

**Trade-off.** One more endpoint to document. Worth it for any field with non-trivial transition rules.

### 7. Two endpoints for tracking — one authenticated, one public

**What.** `GET /api/packages/{id}/tracking` requires authentication and returns full tracking events including employee name and notes. `GET /api/tracking/{id}` is `[AllowAnonymous]` and returns a reduced shape: status + last 5 events with timestamp, status, and store name only.

**Why.** Real postal tracking is public — paste a number into a public page, see where your package is. But the full event log includes internal staff identity and free-text notes that may contain sensitive info ("recipient was not home at delivery attempt"). Two routes with different DTOs is cleaner than one route with conditional shape.

**Trade-off.** Two service methods to maintain instead of one. The public route is also an enumeration vector — see Known limitations and the rate-limiting commit.

### 8. `MapInboundClaims = false` on JwtBearer

**What.** `Program.cs` sets `options.MapInboundClaims = false` in the `AddJwtBearer` configuration.

**Why.** ASP.NET Core's bearer middleware defaults to remapping `sub` → `ClaimTypes.NameIdentifier` and `email` → `ClaimTypes.Email` for backwards compatibility with WS-Federation code. Any service that reads `Principal.FindFirstValue(JwtRegisteredClaimNames.Sub)` after the remap returns null because the claim is no longer named `sub`. This silently breaks ownership checks. Disabling the remap keeps claim names end-to-end identical to what the token-generation code wrote.

**Trade-off.** None at this scale. Existing code that relied on the long-form claim names (none in this project) would need updating.

### 9. Postgres host pinned to `127.0.0.1` rather than `localhost`

**What.** `appsettings.json` connection string uses `Host=127.0.0.1` not `Host=localhost`.

**Why.** On Windows + Docker Desktop + WSL2, `localhost` resolves to both `::1` (IPv6) and `127.0.0.1` (IPv4) — IPv6 wins by default. Docker's port forwarder binds `0.0.0.0:5432` (IPv4 only). WSL2's `wslrelay.exe` binds `[::1]:5432` and can become stale after a `docker restart`, accepting TCP but failing the Postgres protocol handshake. Forcing IPv4 via `127.0.0.1` routes through the healthy path. Confirmed via raw Postgres `SSLRequest` probe: IPv4 replies `N` (proceed plaintext), IPv6 times out.

**Trade-off.** Slightly less idiomatic than `localhost`. A future maintainer might "modernize" it back to `localhost` and break their dev environment. The rationale is documented inline in source and in this README.

### 10. `OutForDelivery` enum value appended (=5) instead of renumbered

**What.** `PackageStatus` is `Created=0, InTransit=1, Delivered=2, Returned=3, Lost=4, OutForDelivery=5`. The logical progression places `OutForDelivery` between `InTransit` and `Delivered`, but the integer value is at the end.

**Why.** The enum is stored as `int` in Postgres. Renumbering would require a data migration (`UPDATE Packages SET Status = Status + 1 WHERE Status >= 2`). Appending costs nothing.

**Trade-off.** The enum source code shows values out of logical order. Mitigated by a comment block on the enum and by the fact that the API surface uses string names via `JsonStringEnumConverter` — callers never see the integer value, so the ordering only matters to anyone reading the C# source.

### 11. Strict JSON deserialization (`UnmappedMemberHandling = Disallow`)

**What.** `AddControllers().AddJsonOptions(...)` sets `JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow`. Unknown JSON properties on a request fail with 400 + a clear message pointing at the offending field.

**Why.** The default is to silently drop unknown properties. That hides typos ("phoneNumer" vs "phoneNumber") and stale client schemas. Strict mode caught a real bug during development — a client sending `name` to a Store create endpoint that didn't yet have a Name field, where the field was silently dropped and the bug was misdiagnosed as a mapping issue.

**Trade-off.** Forward-compat with older clients is harder — a v1 client sending a v2 field would now fail. For a portfolio API with no public clients yet, the defensive default is worth more than the forward-compat flexibility.

### 12. Generic 401 for both bad password and unknown email

**What.** `POST /api/auth/login` returns the same 401 with body `{"title":"Invalid credentials","detail":"The email or password is incorrect."}` whether the email doesn't exist or the password is wrong.

**Why.** Different responses leak whether an email is registered, enabling enumeration attacks against the user base. Same response for both is the security best practice.

**Trade-off.** Slightly worse user experience — the legitimate user can't tell whether they typo'd the email or the password. The trade-off is well-understood; modern auth flows handle this with email-based "did you forget your password?" links rather than telling the attacker which case it is.

### 13. Server-set `senderId` from JWT for Customer-created packages

**What.** When a Customer hits `POST /api/packages`, the `senderId` field in their request body is ignored. The service sets `senderId` from the linked Customer.Id derived from the JWT's `sub` claim. (Admins and Supervisors creating packages on behalf of any customer can supply `senderId` in the body and the server validates it exists.)

**Why.** A Customer could otherwise spoof `senderId=99999` and create a package "from" someone else, attaching its cost to their account or framing them in tracking history. Server-set authority on identity-derived fields is the only safe pattern.

**Trade-off.** None. This is just how identity-derived fields should work; the alternative is a vulnerability.

### 14. Customer soft-delete leaves orphan User row (acknowledged wart)

**What.** When `DELETE /api/customers/{id}` flips `Customer.Deleted = true`, the linked `User` row remains live and active. The deleted customer can still log in.

**Why.** True cascade-soft-delete (mark User as deleted too, invalidate sessions, prevent re-login) requires more machinery than a 7-day portfolio build justifies: a token revocation list, a User.Deleted query filter coordinated with the auth pipeline, and a decision about what happens to the email address (released for re-registration? Reserved forever?).

**Trade-off.** Acknowledged as a wart. Production fix would either: (a) cascade-soft-delete the User and add it to a token blocklist with TTL until any outstanding JWT expires; or (b) leave the Customer record but null its `UserId` and mark the User itself deleted, blocking login. Both involve coordinating across services (auth, customer, JWT validation) that are out of scope here.

## Known limitations

These are the gaps that matter in production but didn't justify implementation in a 7-day portfolio build. Each has a defined production fix; flagging them here means a reviewer doesn't think they were missed.

### Anonymous tracking endpoint enumeration risk

`GET /api/tracking/{id}` uses sequential integer IDs. With per-IP rate limiting (added in this build at 60 req/min), a casual attacker can't enumerate the entire system, but a determined one with a botnet still can. Production fix: switch from sequential `int` primary keys to non-sequential public tracking codes (UUIDs or short random alphanumeric strings) generated separately from the DB primary key. With 1B+ value space, enumeration becomes economically infeasible. The current rate limit is per-IP only and does nothing against distributed enumeration.

### Customer soft-delete orphans the linked User

See architecture decision #14. The deleted customer can still authenticate. Production fix described there.

### No automated tests

Service classes are interface-injected and structured for testability, but no xUnit suite exists. Every endpoint is verified manually via curl smoke tests captured in commit messages. Adding tests is a 1-2 day expansion.

## Local development tips

Reset the database from scratch:
```bash
docker compose down -v
docker compose up -d
dotnet ef database update --project UHPS.API
```

Wipe test data without resetting:
```bash
docker exec uhps-postgres psql -U uhps_admin -d uhps_db -c \
  'DELETE FROM "TrackingRecords"; DELETE FROM "Packages"; DELETE FROM "Customers" WHERE "Id" > 1;'
```

Generate a new BCrypt hash for the seeded admin (if rotating the dev password):
```bash
# Run inline using BCrypt.Net-Next from the project
dotnet run --project /tmp/bcrypt-gen
```

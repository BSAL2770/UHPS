using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Options;
using UHPS.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Strict JSON deserialization: reject requests with unknown properties (400) instead of
// silently dropping them. This catches typo'd fields and stale client schemas in dev.
// Note: production may want to revisit if forward-compat with older clients matters
// (i.e., a v1 client sending a v2 field would start failing).
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste your JWT here. Example: Bearer eyJhbGciOi...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Connection string sourcing:
// - Production / managed-host deployments set DATABASE_URL in URI form; convert to Npgsql key=value.
// - Local dev falls back to appsettings.json's DefaultConnection (Host=127.0.0.1).
// Either way, ConnectionStrings__DefaultConnection env var also works as a final override
// since it goes through the standard config provider.
var connectionString = DatabaseUrl.FromEnvVar()
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "Jwt configuration missing. Set Jwt:Key, Jwt:Issuer, Jwt:Audience via user-secrets or env vars.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep claim names as-issued. By default the bearer middleware maps "sub" to
        // ClaimTypes.NameIdentifier and "email" to ClaimTypes.Email, which silently breaks
        // FindFirstValue(JwtRegisteredClaimNames.Sub) lookups in our CurrentUser service.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();

builder.Services.AddExceptionHandler<ForbiddenAccessExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

// Per-IP rate limiter on the public anonymous tracking endpoint.
// 60 requests per minute per IP — generous for a real user pasting tracking numbers,
// restrictive enough that single-IP enumeration takes ~hours per 10k IDs. Distributed
// enumeration (botnets) defeats this; the proper fix is non-sequential public tracking
// numbers, documented in the README under Known limitations.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("public-tracking", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Auto-apply migrations on startup in non-Development environments. Local dev keeps the
// explicit `dotnet ef database update` workflow so changes are reviewable before applying;
// managed deployments don't have a developer at the console, so the app applies on boot.
app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

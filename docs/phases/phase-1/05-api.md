# Phase 5 — API Layer

## Goal

Thin HTTP surface. Each endpoint resolves the tenant, delegates to one use case, and maps the result to an HTTP response. No business logic, no database calls.

---

## Technical Specification

### 1. Project structure

```
Slot.API/
  Endpoints/
    TenantsEndpoints.cs
    ResourcesEndpoints.cs
    ServiceTypesEndpoints.cs
    SlotsEndpoints.cs
    CustomersEndpoints.cs
    CreditPacksEndpoints.cs
    BookingsEndpoints.cs
  Middleware/
    TenantResolutionMiddleware.cs
  ErrorHandling/
    GlobalExceptionHandler.cs
    ExceptionMappings.cs
  Models/
    Requests/      — inbound DTOs
    Responses/     — outbound DTOs
  Validation/
    Validators/    — one FluentValidation validator per request model
  Program.cs
```

---

### 2. Tenant resolution middleware

**`TenantResolutionMiddleware.cs`**

Reads tenant slug from the URL prefix:

```
Route prefix: /{slug}/...
Context key:  HttpContext.Items["TenantId"] = (int)tenantId
```

```csharp
public async Task InvokeAsync(HttpContext context, GetTenantBySlugHandler handler)
{
    var slug = context.GetRouteValue("slug")?.ToString();
    if (slug is null) { await next(context); return; }

    var tenant = await handler.HandleAsync(new GetTenantBySlugQuery(slug), context.RequestAborted);
    // handler throws NotFoundException if not found/inactive → caught by GlobalExceptionHandler

    context.Items["TenantId"] = tenant.Id;
    await next(context);
}
```

Register before routing in `Program.cs`.

All tenant-scoped endpoint groups use the route prefix `/{slug}`. The public tenant-creation endpoint (`POST /tenants`) is outside this prefix.

---

### 3. Global exception handler

**`GlobalExceptionHandler.cs`** — implements `IExceptionHandler` (ASP.NET Core 8+):

```csharp
public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
{
    var (status, code) = ex switch
    {
        NotFoundException        => (404, "not_found"),
        ConflictException        => (409, "conflict"),
        DomainException          => (409, "domain_rule_violated"),
        ValidationException      => (422, "validation_failed"),
        _                        => (500, "internal_error")
    };

    ctx.Response.StatusCode = status;
    await ctx.Response.WriteAsJsonAsync(new ErrorResponse(code, ex.Message, correlationId), ct);
    return true;
}
```

`correlationId` is taken from `HttpContext.TraceIdentifier`. Stack traces are never included in responses.

Register with `app.UseExceptionHandler()` — first in the middleware pipeline.

---

### 4. Validation

Use FluentValidation. Register validators by assembly scan in `Program.cs`:

```csharp
services.AddValidatorsFromAssemblyContaining<CreateSlotRequest>();
```

Create a minimal API filter that runs validation before the handler:

```csharp
// reusable filter
app.MapPost("/slots", CreateSlot)
   .AddEndpointFilter<ValidationFilter<CreateSlotRequest>>();
```

`ValidationFilter<T>` resolves `IValidator<T>`, calls `ValidateAsync`, and returns `422` with field errors if invalid — the handler is never called.

Validators live in `Slot.API/Validation/Validators/`. Example:

```csharp
public class CreateSlotRequestValidator : AbstractValidator<CreateSlotRequest>
{
    public CreateSlotRequestValidator()
    {
        RuleFor(x => x.ServiceTypeId).GreaterThan(0);
        RuleFor(x => x.StartsAt).GreaterThan(DateTimeOffset.UtcNow);
    }
}
```

---

### 5. Endpoint groups

Each file registers one `RouteGroupBuilder`. All groups are mapped in `Program.cs`.

**`TenantsEndpoints.cs`**

```
POST   /tenants                  → CreateTenantHandler
GET    /tenants/{slug}           → GetTenantBySlugHandler (admin/debug)
```

No `/{slug}` prefix — tenant does not exist yet at creation time.

**`ResourcesEndpoints.cs`** — prefix `/{slug}/resources`

```
GET    /                         → list resources for tenant
POST   /                         → CreateResourceHandler
PUT    /{resourceId}/config      → UpdateResourceConfigHandler
DELETE /{resourceId}             → DeactivateResourceHandler
```

**`ServiceTypesEndpoints.cs`** — prefix `/{slug}/service-types`

```
GET    /
POST   /                         → CreateServiceTypeHandler
PUT    /{id}                     → UpdateServiceTypeHandler
DELETE /{id}                     → DeleteServiceTypeHandler
```

**`SlotsEndpoints.cs`** — prefix `/{slug}/slots`

```
GET    /                         → list slots (filter by date, status)
POST   /                         → CreateSlotHandler
POST   /{slotId}/resources       → AddResourceToSlotHandler
DELETE /{slotId}/resources/{rid} → RemoveResourceFromSlotHandler
POST   /{slotId}/cancel          → CancelSlotHandler
POST   /{slotId}/complete        → CompleteSlotHandler
```

**`CustomersEndpoints.cs`** — prefix `/{slug}/customers`

```
POST   /                         → RegisterCustomerHandler
POST   /{id}/freeze              → UpdateCustomerStatusHandler(Freeze)
POST   /{id}/unfreeze            → UpdateCustomerStatusHandler(Unfreeze)
POST   /{id}/ban                 → UpdateCustomerStatusHandler(Ban)
```

**`CreditPacksEndpoints.cs`** — prefix `/{slug}/customers/{customerId}/credit-packs`

```
POST   /                         → IssueCreditPackHandler
POST   /{packId}/freeze          → FreezeCreditPackHandler
POST   /{packId}/unfreeze        → UnfreezeCreditPackHandler
```

**`BookingsEndpoints.cs`** — prefix `/{slug}/bookings`

```
POST   /                         → BookSlotHandler
POST   /{bookingId}/cancel       → CancelBookingByCustomerHandler
POST   /{bookingId}/cancel-tenant → CancelBookingByTenantHandler
POST   /{bookingId}/no-show      → MarkNoShowHandler
```

---

### 6. Reading `TenantId` in handlers

Extension method to avoid repeating the cast:

```csharp
// Slot.API/Extensions/HttpContextExtensions.cs
public static int GetTenantId(this HttpContext ctx)
    => (int)ctx.Items["TenantId"]!;
```

Usage in endpoint:

```csharp
app.MapPost("/", async (CreateSlotRequest req, HttpContext ctx, CreateSlotHandler handler, CancellationToken ct) =>
{
    var tenantId = ctx.GetTenantId();
    var result = await handler.HandleAsync(new CreateSlotCommand(tenantId, req.ServiceTypeId, req.ResourceIds, req.StartsAt), ct);
    return Results.Created($"/slots/{result.SlotId}", result);
});
```

---

### 7. Dependency injection wiring

`Program.cs` calls extension methods from each layer:

```csharp
builder.Services
    .AddPersistence(sp => ...)        // Slot.Persistence
    .AddConfig(sp => ...)             // Slot.Infrastructure
    .AddMemoryCache()                  // Slot.Cache.Memory
    .AddApplicationHandlers()          // Slot.Application — registers all handlers
    .AddValidatorsFromAssemblyContaining<CreateSlotRequest>();

app.UseExceptionHandler();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseRouting();

app.MapTenantEndpoints();
app.MapResourceEndpoints();
// ...
```

**`Slot.Application/Bootstrap.cs`** — registers all use case handlers as `Scoped`:

```csharp
public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
{
    services.AddScoped<CreateTenantHandler>();
    services.AddScoped<GetTenantBySlugHandler>();
    services.AddScoped<CreateSlotHandler>();
    // ... all handlers
    return services;
}
```

---

### 8. Done when

- [ ] All endpoint groups registered and reachable
- [ ] `TenantResolutionMiddleware` correctly isolates tenants by slug
- [ ] `GlobalExceptionHandler` maps all domain exceptions to correct HTTP codes
- [ ] `ValidationFilter<T>` returns 422 with field-level errors before handler runs
- [ ] No business logic in any endpoint handler — only command construction and `Results.*`
- [ ] End-to-end test: `POST /tenants` → `POST /{slug}/slots` → `POST /{slug}/bookings` → verify credit deducted

---

## Documentation

### What is the API Layer?

The API layer is the HTTP surface of the application. Its only responsibilities are:
- Parse and validate the incoming HTTP request
- Resolve the tenant from the URL
- Call the correct use case handler
- Map the result (or exception) to an HTTP response

No business rules live here. If an endpoint handler contains an `if` checking a business condition, that logic belongs in the Application or Domain layer.

### Minimal API vs Controllers

The project uses **Minimal API** endpoint groups (`RouteGroupBuilder`). This keeps each endpoint small and avoids the ceremony of controller classes while still organising endpoints by aggregate.

### Middleware order

```
ExceptionHandler       ← catches everything below
TenantResolution       ← sets TenantId in HttpContext
Routing
ValidationFilter       ← runs per-endpoint, before handler
Endpoint handler
```

Exception handler must be first so it can catch errors from tenant resolution (e.g. `NotFoundException` for an unknown slug).

### Tenant isolation at the API boundary

The middleware guarantees that `HttpContext.Items["TenantId"]` is always set and valid for any `/{slug}/...` route. Endpoint handlers pass this `TenantId` into their command — use cases then verify it against every loaded entity. This double-check (middleware + use case) ensures that even if the middleware is bypassed or misconfigured, the use case will still reject cross-tenant access.

### Error response format

All errors follow the same envelope:

```json
{
  "code": "domain_rule_violated",
  "message": "Slot is not in Scheduled status.",
  "correlationId": "0HN5K2G2..."
}
```

`correlationId` maps to the ASP.NET Core `HttpContext.TraceIdentifier` — use it to find the request in logs.

### Request / Response DTOs

Request models (in `Models/Requests/`) are plain records with no domain knowledge:

```csharp
public record CreateSlotRequest(
    int ServiceTypeId,
    int[] ResourceIds,
    DateTimeOffset StartsAt);
```

Response models (in `Models/Responses/`) are also plain records. Never return domain entities directly from endpoints — entity internals should not leak into the HTTP contract.

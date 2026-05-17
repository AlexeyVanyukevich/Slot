# Phase 4 — Application Layer

## Goal

All business operations expressed as use cases. The Application layer orchestrates the domain (Phase 1), repositories (Phase 2), and config (Phase 3) — but never depends on EF Core, MongoDB drivers, or HTTP directly.

---

## Technical Specification

### 1. Project references

`Slot.Application.csproj` must reference:
- `Slot.Domain` — entities, enums, exceptions

It must NOT reference:
- `Slot.Persistence`
- `Slot.Infrastructure`

All infrastructure dependencies are injected through interfaces defined in `Slot.Application/Interfaces/`.

---

### 2. Shared infrastructure

**`Slot.Application/Interfaces/Repositories/`** — all repository interfaces (defined here, implemented in `Slot.Persistence`):

```
IRepository<TEntity, TKey>
ISlotRepository
IBookingRepository
ICreditPackRepository
IUnitOfWork
```

**`Slot.Application/Interfaces/Config/`** — config interfaces (defined here, implemented in `Slot.Infrastructure`):

```
ITenantConfigReader
ITenantConfigWriter
IResourceConfigReader
IResourceConfigWriter
```

**Error strategy — exceptions only.** Use cases throw domain exceptions from `Slot.Domain/Exceptions/` or `NotFoundException` (defined in Application) for missing entities. No `Result<T>` wrapper — the API layer catches and maps exceptions to HTTP responses.

```csharp
// Slot.Application/Exceptions/NotFoundException.cs
public sealed class NotFoundException(string entityName, object id)
    : Exception($"{entityName} {id} not found.");
```

---

### 3. Use cases — Tenant

**`CreateTenantCommand`** / **`CreateTenantHandler`**

Input: `Name`, `Slug`

Steps:
1. Check slug uniqueness — query `ITenantRepository.GetBySlugAsync(slug)` → if exists throw `ConflictException`
2. Create `Tenant` entity
3. `tenantRepository.Add(tenant)`
4. `await unitOfWork.SaveChangesAsync()` to get the generated `Id`
5. `await tenantConfigWriter.UpsertAsync(tenant.ConfigId, TenantConfig.Default, ct)`

Output: `TenantId`

**`GetTenantBySlugQuery`** / **`GetTenantBySlugHandler`**

Input: `Slug`

Output: `TenantDto { Id, Name, Slug, IsActive }`

Used by tenant resolution middleware. Throws `NotFoundException` if not found or `IsActive = false`.

---

### 4. Use cases — Resource

**`CreateResourceCommand`** / **`CreateResourceHandler`**

Input: `TenantId`, `Name`

Steps:
1. Verify tenant exists
2. Create `Resource` entity with a new `ConfigReference` (generated `Guid.NewGuid().ToString()` — this becomes the MongoDB `_id`)
3. `resourceRepository.Add(resource)`
4. `await unitOfWork.SaveChangesAsync()`
5. `await resourceConfigWriter.UpsertAsync(resource.ConfigReference, ResourceConfig.Default, ct)`

**`UpdateResourceConfigCommand`** / **`UpdateResourceConfigHandler`**

Input: `TenantId`, `ResourceId`, `ResourceType`, `Metadata`

Steps:
1. Load resource, verify `TenantId` matches (tenant isolation)
2. Build updated `ResourceConfig`
3. `await resourceConfigWriter.UpsertAsync(resource.ConfigReference, config, ct)`

No PostgreSQL write — config only lives in MongoDB.

**`DeactivateResourceCommand`**

Steps:
1. Load resource, verify tenant
2. `resource.Deactivate()` (sets `IsActive = false`)
3. `SaveChanges`

---

### 5. Use cases — ServiceType

**`CreateServiceTypeCommand`** — straightforward entity creation, no side effects.

**`UpdateServiceTypeCommand`**

> `CreditCost` can be updated. This does NOT affect `Booking.CreditsConsumed` on existing bookings.

**`DeleteServiceTypeCommand`**

Steps:
1. Check no future `Scheduled` slots reference this `ServiceType`
2. If found → throw `ConflictException("ServiceType has scheduled slots")`
3. Otherwise soft-delete (if `ISoftDeletable`) or hard-delete

---

### 6. Use cases — Slot

**`CreateSlotCommand`** / **`CreateSlotHandler`**

Input: `TenantId`, `ServiceTypeId`, `ResourceIds[]`, `StartsAt`

Steps:
1. Load `ServiceType`, verify tenant
2. Compute `endsAt = StartsAt + ServiceType.DurationMinutes`
3. If `RequiresResource && ResourceIds.Length == 0` → throw `ValidationException`
4. If `ResourceIds.Length > 0`:
   - Verify all resources belong to the tenant and `IsActive`
   - `slotRepository.FindConflictingSlotIdsAsync(tenantId, resourceIds, startsAt, endsAt)` → if any → throw `ResourceConflictException`
5. Create `Slot`, call `slot.AddResource(resource)` for each
6. `slotRepository.Add(slot)`, `SaveChanges`

**`AddResourceToSlotCommand`**

Steps:
1. Load slot (tracking), verify tenant, verify `Status = Scheduled`
2. Verify resource belongs to tenant
3. Run conflict check excluding the current slot
4. `slot.AddResource(resource)`, `SaveChanges`

**`RemoveResourceFromSlotCommand`**

Steps:
1. Load slot (tracking), verify tenant
2. `slot.RemoveResource(resourceId)`, `SaveChanges`

**`CancelSlotCommand`**

Input: `TenantId`, `SlotId`, `Reason?`

Steps:
1. Load slot with confirmed bookings (tracking)
2. `slot.Cancel(reason)`
3. For each confirmed booking:
   - `booking.CancelByTenant("Slot cancelled")`
   - Load the credit pack that was debited (by `CustomerId` — pick the most recently used active pack OR store `CreditPackId` on `Booking`)
   - `creditPack.Refund(booking.CreditsConsumed)`
4. `SaveChanges`

> **Design decision:** To correctly refund credits, `Booking` should store `CreditPackId`. Add this field to the entity (Phase 1 correction) and the persistence config.

**`CompleteSlotCommand`**

Steps:
1. Load slot with confirmed bookings
2. `slot.Complete()`
3. For each confirmed booking: `booking.MarkAttended()`
4. `SaveChanges`

---

### 7. Use cases — Customer

**`RegisterCustomerCommand`**

Input: `TenantId`, `FirstName`, `LastName`, `Phone`, `Email`, `ExternalAuthId?`

Steps:
1. Check uniqueness of phone and email within tenant
2. Create and save `Customer`

**`UpdateCustomerStatusCommand`**

Input: `TenantId`, `CustomerId`, `Action` (Freeze | Unfreeze | Ban)

Steps:
1. Load customer, verify tenant
2. Call corresponding domain method: `customer.Freeze()` / `Unfreeze()` / `Ban()`
3. `SaveChanges`

---

### 8. Use cases — CreditPack

**`IssueCreditPackCommand`**

Input: `TenantId`, `CustomerId`, `TotalCredits`, `ValidFrom`, `ValidUntil`

Steps:
1. Load tenant config → check `CreditPack.Enabled`; if not → throw `ValidationException`
2. Verify customer belongs to tenant and is `Active`
3. Create and save `CreditPack`

**`FreezeCreditPackCommand`** / **`UnfreezeCreditPackCommand`**

Steps:
1. Load tenant config → check `CreditPack.FreezingEnabled`; if not → throw `ValidationException`
2. Load pack, verify tenant and customer ownership
3. `pack.Freeze(now)` / `pack.Unfreeze(now)`
4. `SaveChanges`

---

### 9. Use cases — Booking

**`BookSlotCommand`** / **`BookSlotHandler`**

Input: `TenantId`, `SlotId`, `CustomerId`

Steps:
1. Load tenant config
2. Load slot with `ServiceType`, verify tenant and `Status = Scheduled`
3. Verify `StartsAt` is in the future
4. `bookingRepository.CountConfirmedAsync(slotId)` — must be `< ServiceType.Capacity`
5. Load customer, verify tenant, verify `Status = Active` → else `CustomerNotActiveException`
6. `creditPackRepository.GetActivePacksAsync(customerId)` — find first with `UsedCredits + ServiceType.CreditCost <= TotalCredits`
7. `pack.Deduct(serviceType.CreditCost)`
8. `Booking.Create(tenantId, customerId, slotId, serviceType.CreditCost, pack.Id)` — set `CreditsConsumed` and `CreditPackId`
9. `bookingRepository.Add(booking)`
10. `SaveChanges` — single commit

**`CancelBookingByCustomerCommand`**

Steps:
1. Load tenant config: check `AllowCustomerCancellation`
2. Load booking with slot, verify ownership
3. Check cancellation window: `slot.StartsAt - now > CancellationDeadlineHours` → else deadline passed
4. `booking.CancelByCustomer(reason)`
5. If `CreditReturnOnLateCancellation || withinDeadline`: load `CreditPack` by `booking.CreditPackId`, `pack.Refund(booking.CreditsConsumed)`
6. `SaveChanges`

**`CancelBookingByTenantCommand`**

Steps:
1. Load booking, verify tenant
2. `booking.CancelByTenant(reason)`
3. Load pack, `pack.Refund(booking.CreditsConsumed)`
4. `SaveChanges`

**`MarkNoShowCommand`**

Steps:
1. Load booking, verify tenant
2. `booking.MarkNoShow()` — no credit refund
3. `SaveChanges`

---

### 10. Done when

- [ ] `Slot.Application` has no references to `Slot.Persistence` or `Slot.Infrastructure`
- [ ] All repository and config interfaces defined in `Slot.Application/Interfaces/`
- [ ] `NotFoundException` and `ConflictException` defined in `Slot.Application/Exceptions/`
- [ ] All use cases implemented with correct tenant isolation checks
- [ ] `Booking` stores `CreditPackId` (add to Phase 1 entity and Phase 2 config)
- [ ] Unit tests for each use case using mocked interfaces
- [ ] `BookSlot` is tested for: full slot, inactive customer, no credits, expired pack, frozen pack

---

## Documentation

### What is the Application Layer?

The Application layer contains use cases — each one represents a single business operation that a user or system can trigger. It orchestrates the domain model, repositories, and config, but contains no business rules itself. Rules live in domain methods; orchestration lives here.

### Use case structure

Each use case is a pair:

```
CreateSlotCommand   — input data (record or class)
CreateSlotHandler   — single public method: HandleAsync(command, cancellationToken)
```

Handlers are registered in DI and injected into API endpoints. One handler per use case.

### Tenant isolation

Every handler that operates on tenant-scoped data must verify that the loaded entity's `TenantId` matches the `TenantId` from the resolved tenant context. A request from tenant A must never be able to read or modify tenant B's data, even with a valid entity ID.

```csharp
if (slot.TenantId != command.TenantId)
    throw new NotFoundException(nameof(Slot), command.SlotId);
```

Throwing `NotFoundException` (not `ForbiddenException`) on a tenant mismatch prevents ID enumeration.

### Single `SaveChanges` per use case

All repository mutations within one use case are staged in EF Core's change tracker and committed with a single `IUnitOfWork.SaveChangesAsync()` at the end. This gives atomicity: if anything fails before the commit, nothing is written.

Do not call `SaveChanges` in the middle of a use case.

### `CreditPackId` on `Booking`

The booking records which credit pack was debited at booking time (`CreditPackId`). This is required for correct refund on cancellation — we must refund to the same pack that was charged, even if the customer has acquired other packs since.

### Config-gated features

Some features are optional and controlled by tenant config:

| Feature | Config flag | Behaviour when disabled |
|---|---|---|
| Credit packs | `CreditPack.Enabled` | `IssueCreditPack` throws |
| Pack freezing | `CreditPack.FreezingEnabled` | Freeze/Unfreeze throws |
| Customer cancellation | `Booking.AllowCustomerCancellation` | `CancelByCustomer` throws |
| Credit return on late cancel | `Booking.CreditReturnOnLateCancellation` | Credits not refunded if past deadline |

This allows the same platform to behave differently for different business types without code branching.

# Phase 1 — Domain Layer

## Core concepts

The model is intentionally generic. Domain-specific language (e.g. "trainer", "class")
lives only in the tenant configuration — not in the data model.

---

## Entities

### `Tenant`

Represents a business using the platform.

| Field     | Type          | Description                                    |
| --------- | ------------- | ---------------------------------------------- |
| Id        | int           | Unique identifier                              |
| Name      | string        | Business name                                  |
| Slug      | string        | URL-friendly identifier (unique)               |
| ConfigId  | string        | Reference to tenant config document in MongoDB |
| CreatedAt | DateTimeOffset | Registration date                             |
| IsActive  | bool          | Whether the tenant is enabled                  |

---

### `Customer`

A person who makes bookings. Scoped to a tenant.

| Field     | Type            | Description                      |
| --------- | --------------- | -------------------------------- |
| Id        | int             | Unique identifier                |
| TenantId  | int             | FK → Tenant                      |
| FirstName | string          | First name                       |
| LastName  | string          | Last name                        |
| Phone     | string          | Phone number (unique per tenant) |
| Email     | string          | Email (unique per tenant)        |
| CreatedAt | DateTimeOffset  | Registration date                |
| Status    | CustomerStatus  | Active / Frozen / Banned         |
| IsDeleted | bool            | Soft-delete flag                 |

---

### `Resource`

Anything that can be assigned to a slot — a person or a physical space.

| Field           | Type           | Description                                                       |
| --------------- | -------------- | ----------------------------------------------------------------- |
| Id              | int            | Unique identifier                                                 |
| TenantId        | int            | FK → Tenant                                                       |
| Name            | string         | Display name (e.g. "Anna", "Room 3")                              |
| ConfigReference | string         | Reference to resource config document in MongoDB (type, metadata) |
| IsActive        | bool           | Whether the resource is available                                 |
| CreatedAt       | DateTimeOffset | Creation date                                                     |

> Resource type (e.g. Staff / Space) and any specialization metadata are stored in the tenant-scoped config document referenced by `ConfigReference`, not as fields on the entity.

---

### `ServiceType`

Defines what can be booked — a type of service offered by the tenant.

| Field            | Type           | Description                                   |
| ---------------- | -------------- | --------------------------------------------- |
| Id               | int            | Unique identifier                             |
| TenantId         | int            | FK → Tenant                                   |
| Name             | string         | Name (e.g. "Yoga", "Haircut", "Consultation") |
| DurationMinutes  | int            | Duration of one slot                          |
| Capacity         | int            | Max participants per slot (1 = individual)    |
| RequiresResource | bool           | Whether a resource must be assigned           |
| CreditCost       | int            | Credits deducted per booking (default: 1)     |
| Description      | string?        | Optional description                          |
| CreatedAt        | DateTimeOffset | Creation date                                 |

---

### `Slot`

A scheduled instance of a service — a concrete bookable event.

| Field         | Type           | Description                       |
| ------------- | -------------- | --------------------------------- |
| Id            | int            | Unique identifier                 |
| TenantId      | int            | FK → Tenant                       |
| ServiceTypeId | int            | FK → ServiceType                  |
| StartsAt      | DateTimeOffset | Start time                        |
| Status        | SlotStatus     | Scheduled / Cancelled / Completed |
| CancelReason  | string?        | Reason if cancelled               |
| CreatedAt     | DateTimeOffset | Creation date                     |

> `EndsAt` is computed as `StartsAt + ServiceType.DurationMinutes`

> Resources are linked via a many-to-many relationship through `SlotResource`. A slot may have zero or more resources (e.g. a trainer and a room simultaneously).

---

### `SlotResource`

Join entity linking slots to their assigned resources.

| Field      | Type | Description   |
| ---------- | ---- | ------------- |
| SlotId     | int  | FK → Slot     |
| ResourceId | int  | FK → Resource |

> Primary key is composite: `(SlotId, ResourceId)`

---

### `Booking`

A customer's reservation for a specific slot.

| Field           | Type           | Description                                                             |
| --------------- | -------------- | ----------------------------------------------------------------------- |
| Id              | int            | Unique identifier                                                       |
| TenantId        | int            | FK → Tenant                                                             |
| CustomerId      | int            | FK → Customer                                                           |
| SlotId          | int            | FK → Slot                                                               |
| CreditPackId    | int            | FK → CreditPack (the pack that was debited)                             |
| Status          | BookingStatus  | Confirmed / CancelledByCustomer / CancelledByTenant / NoShow / Attended |
| CreditsConsumed | int            | Credits deducted at booking time (snapshot of ServiceType.CreditCost)   |
| BookedAt        | DateTimeOffset | When the booking was created                                            |
| CancelledAt     | DateTimeOffset? | When it was cancelled                                                  |
| CancelReason    | string?        | Reason for cancellation (set by tenant or customer)                     |

---

### `CreditPack`

A bundle of credits purchased by a customer, used to pay for bookings.

| Field        | Type            | Description                                                     |
| ------------ | --------------- | --------------------------------------------------------------- |
| Id           | int             | Unique identifier                                               |
| TenantId     | int             | FK → Tenant                                                     |
| CustomerId   | int             | FK → Customer                                                   |
| TotalCredits | int             | Credits purchased                                               |
| UsedCredits  | int             | Credits consumed                                                |
| ValidFrom    | DateTimeOffset  | Pack activation date                                            |
| ValidUntil   | DateTimeOffset  | Pack expiry date (extended by freeze duration upon unfreeze)    |
| IsFrozen     | bool            | Whether the pack is paused                                      |
| FrozenAt     | DateTimeOffset? | When the freeze started (used to compute extension on unfreeze) |

---

## Enums

```csharp
enum CustomerStatus { Active, Frozen, Banned }
enum SlotStatus     { Scheduled, Cancelled, Completed }
enum BookingStatus  { Confirmed, CancelledByCustomer, CancelledByTenant, NoShow, Attended }
```

---

## Entity relationships

```
Tenant
  ├── Customer (1:N)
  │     ├── Booking (1:N)
  │     └── CreditPack (1:N)
  ├── Resource (1:N)
  ├── ServiceType (1:N)
  └── Slot (1:N)
        ├── ServiceType (N:1)
        ├── Resource (N:M, via SlotResource)
        └── Booking (1:N)
```

---

## Design notes

### Entity design

Entities use **private setters** and **explicit mutation methods**. This ensures that state changes always go through a method that can enforce invariants. Public setters would allow any part of the codebase to put an entity into an invalid state silently.

```csharp
// Wrong — status can be set to anything from anywhere
slot.Status = SlotStatus.Completed;

// Correct — the rule "only Scheduled slots can be completed" is enforced in one place
slot.Complete();
```

### Domain exceptions

Domain exceptions represent rule violations, not technical errors. They are part of the domain API — callers are expected to handle them. They map to HTTP 409 Conflict at the API boundary, not 500 Internal Server Error.

### `SlotResource` — join entity

`Slot` and `Resource` have a many-to-many relationship. An explicit `SlotResource` entity is used rather than an implicit EF Core join to keep the model visible and to allow future addition of fields (e.g. `AssignedAt`, `Role`).

A slot with zero resources is valid when `ServiceType.RequiresResource = false`. Validation of this rule lives in the Application layer (use case), not in the entity — the entity does not know about its `ServiceType`.

### `CreditsConsumed` and `CreditPackId` on `Booking`

`ServiceType.CreditCost` can change over time. When a booking is made, the credit cost is snapshotted into `Booking.CreditsConsumed`. This is the amount refunded on cancellation, regardless of what the current `CreditCost` is.

`CreditPackId` records which pack was debited. On cancellation, credits are refunded to that exact pack — not to the customer's newest pack.

### `CreditPack.Unfreeze` — validity extension

When a pack is unfrozen, `ValidUntil` is extended by the exact duration of the freeze:

```
ValidUntil = ValidUntil + (now - FrozenAt)
```

This ensures customers do not lose paid validity time while a pack is paused.

---

## Implementation

### Goal

Pure domain model with zero infrastructure dependencies. All entities enforce their own invariants via domain methods. No EF Core, no HTTP, no MongoDB — only C# and business rules.

---

### Technical Specification

#### 1. Entities — corrections and additions

All entities extend `Entity` (base class with `int Id`). `DateTimeOffset` is used everywhere.

**`Slot.Domain/Entities/SlotResource.cs`** — new join entity

```csharp
public class SlotResource
{
    public int SlotId { get; private set; }
    public Slot Slot { get; private set; } = null!;
    public int ResourceId { get; private set; }
    public Resource Resource { get; private set; } = null!;
}
```

No separate `Id` — composite PK `(SlotId, ResourceId)` is configured at the persistence layer.

**`Slot.Domain/Entities/Slot.cs`** — remove `ResourceId` / `Resource`, add `SlotResources` collection

```csharp
// Remove:
public int? ResourceId { get; set; }
public Resource? Resource { get; set; }

// Add:
public ICollection<SlotResource> SlotResources { get; } = [];
```

**`Slot.Domain/Entities/Booking.cs`** — add missing fields

```csharp
public int CreditPackId { get; private set; }
public int CreditsConsumed { get; private set; }
public string? CancelReason { get; private set; }
```

**`Slot.Domain/Entities/Resource.cs`** — implement `IActivatable`, add `CreatedAt` via `IAuditable`

```csharp
public class Resource : Entity, IActivatable, IAuditable
{
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; set; }   // set by AuditableInterceptor
    ...
}
```

---

#### 2. Domain exceptions

Create `Slot.Domain/Exceptions/` with one file per exception:

| Exception class | When thrown |
|---|---|
| `SlotNotScheduledException` | Slot operation on a non-`Scheduled` slot |
| `SlotCapacityExceededException` | Booking attempt when slot is full |
| `CustomerNotActiveException` | Booking attempt by a Frozen/Banned customer |
| `InsufficientCreditsException` | No active pack with enough credits |
| `CreditPackExpiredException` | Pack's `ValidUntil` is in the past |
| `CreditPackFrozenException` | Pack is frozen at booking time |
| `ResourceConflictException` | Resource already assigned to an overlapping slot |

All extend a common `DomainException : Exception` base for easy catch-all at the API layer.

---

#### 3. Domain methods

Change all entity setters to `private set` or `init`. All mutations go through explicit methods.

**`Slot`**

```csharp
public void Cancel(string? reason)
    // guard: Status must be Scheduled → throw SlotNotScheduledException

public void Complete()
    // guard: Status must be Scheduled → throw SlotNotScheduledException

public void AddResource(Resource resource)
    // adds SlotResource; idempotent if already present

public void RemoveResource(int resourceId)
    // removes SlotResource; no-op if not present
```

**`Booking`**

```csharp
// Factory — called from BookSlot use case
public static Booking Create(int tenantId, int customerId, int slotId, int creditPackId, int creditsConsumed)

public void CancelByCustomer(string? reason = null)
    // sets Status = CancelledByCustomer, CancelledAt = now, CancelReason

public void CancelByTenant(string? reason = null)
    // sets Status = CancelledByTenant, CancelledAt = now, CancelReason

public void MarkAttended()
    // guard: Status must be Confirmed

public void MarkNoShow()
    // guard: Status must be Confirmed
```

**`Customer`**

```csharp
public void Freeze()    // Status = Frozen
public void Unfreeze()  // Status = Active
public void Ban()       // Status = Banned
```

**`CreditPack`**

```csharp
public void Freeze(DateTimeOffset now)
    // guard: not already frozen
    // IsFrozen = true, FrozenAt = now

public void Unfreeze(DateTimeOffset now)
    // guard: must be frozen
    // ValidUntil += (now - FrozenAt), IsFrozen = false, FrozenAt = null

public void Deduct(int amount)
    // guard: UsedCredits + amount <= TotalCredits → throw InsufficientCreditsException
    // guard: not frozen → throw CreditPackFrozenException
    // guard: not expired → throw CreditPackExpiredException
    // UsedCredits += amount

public void Refund(int amount)
    // UsedCredits -= amount; floor at 0
```

---

### Done when

- [ ] All entities compile with no references outside `Slot.Domain`
- [ ] `Slot` no longer has `ResourceId` / `Resource`, has `SlotResources` collection
- [ ] `Booking` has `CreditPackId`, `CreditsConsumed`, `CancelReason`
- [ ] `SlotResource` join entity exists
- [ ] All domain exceptions defined under `DomainException`
- [ ] All domain methods exist with guard conditions
- [ ] Unit tests cover every method including exception paths

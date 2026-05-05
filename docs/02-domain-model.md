# Slot — Domain Model

## Core concepts

The model is intentionally generic. Domain-specific language (e.g. "trainer", "class")
lives only in the tenant configuration — not in the data model.

---

## Entities

### `Tenant`

Represents a business using the platform.

| Field     | Type     | Description                                              |
| --------- | -------- | -------------------------------------------------------- |
| Id        | Guid     | Unique identifier                                        |
| Name      | string   | Business name                                            |
| Slug      | string   | URL-friendly identifier (e.g. `my-gym`)                  |
| ConfigId  | string   | Reference to tenant config document in MongoDB           |
| CreatedAt | DateTime | Registration date                                        |
| IsActive  | bool     | Whether the tenant is enabled                            |

---

### `Customer`

A person who makes bookings. Scoped to a tenant.

| Field     | Type           | Description                      |
| --------- | -------------- | -------------------------------- |
| Id        | Guid           | Unique identifier                |
| TenantId  | Guid           | FK → Tenant                      |
| FirstName | string         | First name                       |
| LastName  | string         | Last name                        |
| Phone     | string         | Phone number (unique per tenant) |
| Email     | string         | Email (unique per tenant)        |
| CreatedAt | DateTime       | Registration date                |
| Status    | CustomerStatus | Active / Frozen / Banned         |

---

### `Resource`

Anything that can be assigned to a slot — a person or a physical space.

| Field           | Type    | Description                                                       |
| --------------- | ------- | ----------------------------------------------------------------- |
| Id              | Guid    | Unique identifier                                                 |
| TenantId        | Guid    | FK → Tenant                                                       |
| Name            | string  | Display name (e.g. "Anna", "Room 3")                              |
| ConfigReference | string  | Reference to resource config document in MongoDB (type, metadata) |
| IsActive        | bool    | Whether the resource is available                                 |

> Resource type (e.g. Staff / Space) and any specialization metadata are stored in the tenant-scoped config document referenced by `ConfigReference`, not as fields on the entity.

---

### `ServiceType`

Defines what can be booked — a type of service offered by the tenant.

| Field            | Type    | Description                                   |
| ---------------- | ------- | --------------------------------------------- |
| Id               | Guid    | Unique identifier                             |
| TenantId         | Guid    | FK → Tenant                                   |
| Name             | string  | Name (e.g. "Yoga", "Haircut", "Consultation") |
| DurationMinutes  | int     | Duration of one slot                          |
| Capacity         | int     | Max participants per slot (1 = individual)    |
| RequiresResource | bool    | Whether a resource must be assigned           |
| CreditCost       | int     | Credits deducted per booking (default: 1)     |
| Description      | string? | Optional description                          |

---

### `Slot`

A scheduled instance of a service — a concrete bookable event.

| Field         | Type       | Description                       |
| ------------- | ---------- | --------------------------------- |
| Id            | Guid       | Unique identifier                 |
| TenantId      | Guid       | FK → Tenant                       |
| ServiceTypeId | Guid       | FK → ServiceType                  |
| ResourceId    | Guid?      | FK → Resource (optional)          |
| StartsAt      | DateTime   | Start time                        |
| Status        | SlotStatus | Scheduled / Cancelled / Completed |
| CancelReason  | string?    | Reason if cancelled               |

> `EndsAt` is computed as `StartsAt + ServiceType.DurationMinutes`

---

### `Booking`

A customer's reservation for a specific slot.

| Field       | Type          | Description                                                             |
| ----------- | ------------- | ----------------------------------------------------------------------- |
| Id          | Guid          | Unique identifier                                                       |
| TenantId    | Guid          | FK → Tenant                                                             |
| CustomerId  | Guid          | FK → Customer                                                           |
| SlotId      | Guid          | FK → Slot                                                               |
| Status      | BookingStatus | Confirmed / CancelledByCustomer / CancelledByTenant / NoShow / Attended |
| BookedAt    | DateTime      | When the booking was created                                            |
| CancelledAt | DateTime?     | When it was cancelled                                                   |

---

### `CreditPack`

A bundle of credits purchased by a customer, used to pay for bookings.

| Field        | Type      | Description                |
| ------------ | --------- | -------------------------- |
| Id           | Guid      | Unique identifier          |
| TenantId     | Guid      | FK → Tenant                |
| CustomerId   | Guid      | FK → Customer              |
| TotalCredits | int       | Credits purchased          |
| UsedCredits  | int       | Credits consumed           |
| ValidFrom    | DateTime  | Pack activation date       |
| ValidUntil   | DateTime  | Pack expiry date           |
| IsFrozen     | bool      | Whether the pack is paused |
| FrozenAt     | DateTime? | When the freeze started    |

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
        ├── Resource (N:1, optional)
        └── Booking (1:N)
```

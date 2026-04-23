# SlotWise — Tenant Configuration

## Overview

Each tenant has a `ConfigJson` field that controls how the platform behaves
for their domain. This drives terminology, booking rules, and feature flags
without changing any code.

---

## v1 — NoSQL Config (current)

The config is stored as a document in MongoDB and referenced from the `Tenant` entity
via `ConfigId`. It is loaded on demand (or cached per request) and edited manually
or via a future Admin API.

### Full config schema

```json
{
  "terminology": {
    "customer": "Client",
    "resource": "Trainer",
    "serviceType": "Class",
    "slot": "Session",
    "creditPack": "Membership"
  },
  "booking": {
    "cancellationDeadlineHours": 2,
    "allowCustomerCancellation": true,
    "creditReturnOnLateCancellation": false
  },
  "creditPack": {
    "enabled": true,
    "freezingEnabled": true,
    "expiryExtendedOnFreeze": true
  },
  "slot": {
    "requiresResource": true,
    "allowGroupBookings": true
  }
}
```

---

## Config fields

### `terminology`
Renames domain concepts for display purposes (API labels, error messages, future UI).

| Key | Default | Example values |
|---|---|---|
| customer | "Customer" | "Client", "Patient", "Guest" |
| resource | "Resource" | "Trainer", "Doctor", "Barber" |
| serviceType | "Service" | "Class", "Treatment", "Lesson" |
| slot | "Slot" | "Session", "Appointment", "Booking" |
| creditPack | "Credit Pack" | "Membership", "Bundle", "Pass" |

---

### `booking`

| Key | Type | Description |
|---|---|---|
| cancellationDeadlineHours | int | Hours before slot start when cancellation is no longer free |
| allowCustomerCancellation | bool | Whether customers can cancel their own bookings |
| creditReturnOnLateCancellation | bool | Whether credits are returned if cancelled after deadline |

---

### `creditPack`

| Key | Type | Description |
|---|---|---|
| enabled | bool | Whether credit packs are used at all |
| freezingEnabled | bool | Whether customers can freeze their pack |
| expiryExtendedOnFreeze | bool | Whether `ValidUntil` is extended by the freeze duration |

---

### `slot`

| Key | Type | Description |
|---|---|---|
| requiresResource | bool | Whether every slot must have a resource assigned |
| allowGroupBookings | bool | Whether slots can accept more than one booking (capacity > 1) |

---

## Domain examples

### Fitness studio
```json
{
  "terminology": { "customer": "Client", "resource": "Trainer", "serviceType": "Class", "slot": "Session", "creditPack": "Membership" },
  "booking": { "cancellationDeadlineHours": 2, "allowCustomerCancellation": true, "creditReturnOnLateCancellation": false },
  "creditPack": { "enabled": true, "freezingEnabled": true, "expiryExtendedOnFreeze": true },
  "slot": { "requiresResource": true, "allowGroupBookings": true }
}
```

### Barbershop
```json
{
  "terminology": { "customer": "Client", "resource": "Barber", "serviceType": "Service", "slot": "Appointment", "creditPack": "Bundle" },
  "booking": { "cancellationDeadlineHours": 1, "allowCustomerCancellation": true, "creditReturnOnLateCancellation": true },
  "creditPack": { "enabled": false, "freezingEnabled": false, "expiryExtendedOnFreeze": false },
  "slot": { "requiresResource": true, "allowGroupBookings": false }
}
```

### Medical clinic
```json
{
  "terminology": { "customer": "Patient", "resource": "Doctor", "serviceType": "Consultation", "slot": "Appointment", "creditPack": "Pass" },
  "booking": { "cancellationDeadlineHours": 24, "allowCustomerCancellation": true, "creditReturnOnLateCancellation": true },
  "creditPack": { "enabled": true, "freezingEnabled": false, "expiryExtendedOnFreeze": false },
  "slot": { "requiresResource": true, "allowGroupBookings": false }
}
```

---

## Roadmap

| Version | What changes |
|---|---|
| v1 | Config document per tenant stored in MongoDB, referenced by `ConfigId` |
| v2 | Admin API — CRUD endpoints to read and update config at runtime |
| v3 | Pre-built templates selectable at tenant registration (fitness, barbershop, clinic, coworking) |

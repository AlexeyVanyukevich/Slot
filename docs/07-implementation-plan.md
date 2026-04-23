# SlotWise — Implementation Plan

---

## Phase 1 — Domain layer

Goal: model the core concepts and rules with no dependencies.

- [ ] Create the .NET solution and 4 projects
- [ ] Implement all domain entities (`Tenant`, `Customer`, `Resource`, `ServiceType`, `Slot`, `Booking`, `CreditPack`)
- [ ] Add enums (`CustomerStatus`, `ResourceType`, `SlotStatus`, `BookingStatus`)
- [ ] Define `TenantConfig` value object (deserialized from JSON)
- [ ] Add domain exceptions (`SlotFullException`, `BookingConflictException`, `CancellationDeadlineException`, `InsufficientCreditsException`)
- [ ] Write unit tests for all business rules (no DB, no HTTP)

**Milestone:** all booking rules are tested and pass in isolation.

---

## Phase 2 — Persistence (Infrastructure)

Goal: store and query domain entities using EF Core + PostgreSQL.

- [ ] Set up `SlotWiseDbContext` with `TenantId` global query filters
- [ ] Write EF entity type configurations for all entities
- [ ] Create the initial migration
- [ ] Implement repositories (`BookingRepository`, `SlotRepository`, `CustomerRepository`, `CreditPackRepository`)
- [ ] Implement `TenantConfigProvider` (reads + deserializes `ConfigJson`)
- [ ] Add `IUnitOfWork` and `UnitOfWork` implementation

**Milestone:** data persists and queries are isolated per tenant.

---

## Phase 3 — Application layer

Goal: implement use cases that orchestrate domain logic.

- [ ] `CreateBookingHandler` — validate slot, check credits, create booking
- [ ] `CancelBookingHandler` — check deadline, return credits if applicable
- [ ] `CancelSlotHandler` — cancel all bookings, return credits to all customers
- [ ] `CompleteSlotHandler` — mark slot complete, trigger Attended/NoShow flow
- [ ] `AddCreditPackHandler`
- [ ] `FreezePackHandler` / `UnfreezePackHandler` (extend ValidUntil if configured)

**Milestone:** all use cases work end-to-end with a real database.

---

## Phase 4 — API layer

Goal: expose everything over HTTP.

- [ ] `TenantResolutionMiddleware` — resolve `X-Tenant-Id` header, inject `ITenantContext`
- [ ] `ExceptionHandlingMiddleware` — map `DomainException` subtypes to correct HTTP status codes
- [ ] Implement all controllers (Tenants, Customers, Resources, ServiceTypes, Slots, Bookings, CreditPacks)
- [ ] Add FluentValidation for all request DTOs
- [ ] Configure Swagger / Scalar with `X-Tenant-Id` header support
- [ ] Add integration tests for the most critical flows (book → cancel → complete)

**Milestone:** full API is functional and documented.

---

## Phase 5 — v2: Admin API for config (future)

Goal: allow tenants to update their configuration via API without editing JSON manually.

- [ ] `GET /tenants/{id}/config` — return typed config object
- [ ] `PUT /tenants/{id}/config` — validate and save config
- [ ] `PATCH /tenants/{id}/config/booking` — partial update for booking rules
- [ ] Config validation (e.g. `cancellationDeadlineHours` must be >= 0)
- [ ] Config change audit log

---

## Phase 6 — v3: Domain templates (future)

Goal: let new tenants pick a pre-built configuration at registration.

- [ ] Define built-in templates: `fitness`, `barbershop`, `clinic`, `coworking`, `tutoring`
- [ ] `GET /templates` — list available templates with descriptions
- [ ] `POST /tenants` accepts optional `templateId` field
- [ ] Template is copied into `ConfigJson` at tenant creation (can be customized afterwards)

---

## Suggested build order

```
Phase 1 → Phase 2 → Phase 3 → Phase 4 → (ship v1)
                                              ↓
                                         Phase 5 → Phase 6
```

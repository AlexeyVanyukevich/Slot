# Universal Booking Platform
## Business Requirements and MVP Plan

---

## Part I. General Concept

The goal is to build a booking engine where the core mechanics (tenants, resources, time, booking statuses) are the same for everyone, while domain-specific details (what is being booked, what data is collected, what rules apply) are extracted into tenant configuration. The same product serves house rentals, fitness class sign-ups, table reservations, service appointments, and so on.

### Key Entities

- **Tenant** — a business owner (a trainer, a homeowner, a studio). Stores their own configuration, resources, and rules.
- **Resource** — what is being booked. Can be a single unique item (one specific house) or a type with multiple instances (10 identical hotel rooms).
- **Availability Slot** — a time window in which a resource can be booked.
- **Booking** — the act of reserving a specific resource at a specific time, with data filled in according to the tenant's schema.
- **Client** — the person who books. Can be a guest without an account or a registered user.
- **Booking Configuration** — the field schema that the tenant defines themselves.

### Core Architectural Principles

1. Strict separation of "domain mechanics" (shared by all tenants) and "configuration" (tenant-specific).
2. Tenant data is isolated (multi-tenant isolation).
3. Any schema operation by a tenant must not break existing bookings.
4. The server is the single source of truth for rules and validation; the client validates "for convenience" only.

---

## Part II. Tenant Configuration

### Chapter 1. Two Levels of Configuration

**Resource configuration** — a description of the thing being booked. Filled out by the owner once at creation:
- For a house: address, bedrooms, amenities, photos, rules, minimum rental period.
- For a fitness class: name, description, level, equipment, duration.

**Booking form configuration** — what is collected from the client at the time of booking:
- For a house: check-in/check-out dates, number of guests, purpose of trip, whether pets are present.
- For a fitness class: name, phone, experience level, medical restrictions.

These entities **are not mixed**. The resource describes "what is on offer"; the form describes "what to ask the client."

### Chapter 2. Form Field Definition

Each field is an object with the following properties:
- Identifier (a stable machine-readable name).
- Data type.
- Label, placeholder, help text.
- Required flag.
- Default value.
- Validation rules.
- Visibility and required-state conditions.

### Chapter 3. Field Type Catalog

A finite, controlled set. Extended only when there is a real need.

| Type | Description | Validation examples |
|---|---|---|
| `short_text` | Single-line text | minLength, maxLength, pattern (email, phone) |
| `number` | Integer or decimal | min, max, step |
| `date` | Calendar date | minDate, maxDate, disallowPast |
| `single_select` | Dropdown or radio | list of allowed values |
| `checkbox` | Boolean (yes/no) | — |

**Not in MVP:** long text, multi-select, file upload, phone with country code, date range. Added in Stage 2.

### Chapter 4. Validation Layers

**Layer 1 — Formal validation (per field):**
- Type compliance.
- Required field check.
- Type-specific constraints (length, range, pattern, allowed values).

**Layer 2 — Cross-field validation:**
- Logical consistency between fields (e.g. check-out date after check-in date).
- Defined by rules, not arbitrary code.

**Layer 3 — Business validation:**
- Resource availability for the selected slot.
- Minimum lead time.
- Maximum booking horizon.
- Double-booking prevention.

### Chapter 5. Conditional Logic

Conditions control **field visibility** and **required state**.

A condition is a rule: `IF field X has value Y → THEN field Z is visible / required`.

**Rules:**
- Conditions reference only fields of types `single_select` and `checkbox`.
- Conditions are checked in real time on the client (for UX) and on the server (for integrity).
- A hidden field's value is **not saved** in the booking.
- If a field becomes visible later, the client must fill it in explicitly.

### Chapter 6. Select Options

Options for `single_select` fields are **static** in MVP — defined when the form is created, do not change based on other fields. Dynamic options (loaded from an API) are a post-MVP feature.

### Chapter 7. Schema Versioning

**Problem:** the tenant may change the form after bookings have already been created. Old bookings must remain readable with their original structure.

**Solution:**
- Each published version of the schema is **immutable**.
- When a booking is created, the `schema_version_id` at the time of creation is stored.
- To display an old booking, the system reads the schema version that was current at the time of creation.

**What counts as a breaking change:**
- Removing a required field.
- Changing a field's type.
- Renaming a field identifier.

**What is a compatible change (no new version required in MVP):**
- Changing a label or hint.
- Making a required field optional.
- Adding a new optional field.

**In MVP:** versioning is simplified — a snapshot of values is taken at the time of booking creation, so even if the schema is modified in-place, old bookings retain their original data structure.

---

## Part III. Bookings

### Chapter 8. Booking Lifecycle

```
Pending → Confirmed → Completed
                ↓
           Cancelled
```

- **Pending** — created but not yet confirmed (used when `auto_confirm = false`).
- **Confirmed** — confirmed by the tenant or automatically.
- **Completed** — the booking time has passed and the booking was not cancelled.
- **Cancelled** — cancelled by the client, tenant, or automatically.

All status transitions are recorded in the audit log (`booking_status_history`).

### Chapter 9. Key Decisions

**Guest bookings:**
Guest bookings without registration are supported.

**Storing booking values:**
Structure: `(field_key, value)` pairs + reference to the schema version at the time of creation. Preserves historical data.

**Time zones:**
Always store in UTC. Display in the resource's time zone for the tenant, and in the client's (or resource's) time zone for the client.

**Double-booking protection:**
Enforced at the database layer: a partial unique index on `(resource_id, slot_start_at) WHERE status IN ('Pending', 'Confirmed')`.

---

## Part IV. Notifications

### Chapter 9a. Notification Channels

Notifications are delivered to configurable channels — not to tenant user accounts directly. This allows routing to shared mailboxes, webhooks, or Slack without requiring a `tenant_user` login.

**Channel types (MVP):** `Email`, `Webhook`, `Slack`

**Event types:**
- `BookingCreated` — a new booking has been submitted.
- `BookingConfirmed` — a booking has been confirmed.
- `BookingCancelled` — a booking has been cancelled.
- `BookingRescheduled` — a booking slot has changed.
- `DailySummary` — end-of-day digest.

**Rules:**
- A tenant can have multiple channels (e.g. a reception inbox + a Slack alert).
- Each channel subscribes to a specific set of event types.
- On tenant registration, one default `Email` channel is seeded automatically, pointing to the owner's email with all event types enabled.
- Channels can be enabled/disabled individually without deletion.

**Post-MVP:** SMS, push notifications.

---

## Part V. MVP Scope

### Chapter 10. MVP Phases and Tasks

Estimated timeline for a single full team (1–2 developers + designer). Numbers are approximate.

---

#### Phase 0. Foundation (1–2 weeks)

Infrastructure setup and basic entities.

- [ ] Finalize the technology stack (frontend, backend, DB, queue, email provider).
- [ ] Create the repository, set up CI/CD pipeline (lint, tests, deploy).
- [ ] Define the ER model for core entities: tenant, resource, schedule, slot, booking, schema, schema_field, notification_channel.
- [ ] Implement DB migrations.
- [ ] Set up authentication for tenant users (email + password via OpenIddict).
- [ ] Basic role system (only "Owner" for tenant in MVP).
- [ ] Logging and basic error monitoring.
- [ ] Connect email notifications sandbox.

---

#### Phase 1. Tenant and Resource (1 week)

Tenant can register and create their first resource.

- [ ] Tenant registration (form + email confirmation).
- [ ] Tenant profile page: name, timezone, slug.
- [ ] Notification channels UI: view, add, edit, toggle active state.
- [ ] Default channel seeded automatically on registration.
- [ ] CRUD for a single resource: name, description, photos (1–3).
- [ ] Resource slug (generation and editing).
- [ ] Basic validations on the resource form.

---

#### Phase 2. Form Builder (2 weeks)

Tenant configures what data is collected from the client.

- [ ] Form schema and field model in the DB.
- [ ] Editor UI: field list, add/remove/reorder.
- [ ] Support for 5 field types: `short_text`, `number`, `date`, `single_select`, `checkbox`.
- [ ] Field properties: label, identifier, required flag, hint.
- [ ] Type-based validation (min/max length for text, min/max for number, list of values for select).
- [ ] System fields added automatically (name, phone, email, slot selection).
- [ ] Form preview — tenant sees what the client will see.
- [ ] Schema saving (snapshot of values taken when a booking is created).
- [ ] Warning to tenant: "if you edit the form, new bookings will have the new structure; existing ones remain as-is."

---

#### Phase 3. Schedule and Availability (1–2 weeks)

Tenant configures when the resource is available.

- [ ] Working hours by day of week (e.g. Mon–Fri 9:00–18:00).
- [ ] Slot duration (15/30/60 minutes or custom).
- [ ] Buffer between slots (optional).
- [ ] Blocking specific dates (vacation, holidays, maintenance).
- [ ] Generation of available slots for the next N days.
- [ ] API "get free slots for a date range" — accounts for existing bookings and blocked dates.

---

#### Phase 4. Booking Flow (2 weeks)

Client can select a slot and submit a booking.

- [ ] Public booking page for the resource (by tenant slug + resource slug).
- [ ] Slot picker UI (calendar or list).
- [ ] Dynamic form rendering based on the tenant's schema.
- [ ] Client-side validation (required fields, type rules).
- [ ] Booking submission — server validates and creates the booking.
- [ ] Double-booking protection at the DB level (partial unique index).
- [ ] Email confirmation to the client.
- [ ] Notification dispatch to configured tenant channels on `BookingCreated`.
- [ ] Booking confirmation page (with `external_reference` code, e.g. `BK-2026-00042`).

---

#### Phase 5. Tenant Dashboard (1–2 weeks)

Tenant manages their bookings.

- [ ] Booking list with filters: date, status, search by client name/email.
- [ ] Booking detail page (all field values, status history).
- [ ] Manual confirm / cancel actions.
- [ ] Notification dispatch to configured channels on status changes.
- [ ] Daily summary email (opt-in, via `DailySummary` channel event).
- [ ] Simple landing page for the platform.
- [ ] Deploy to production.
- [ ] Onboard 2 pilot tenants from different domains.

---

### Chapter 11. Post-MVP Roadmap

In priority order, based on pilot feedback:

**Stage 2. Booking Engine Depth:**
- Multiple resources per tenant.
- Multiple instances of a resource (e.g. "10 identical rooms").
- Arbitrary date ranges instead of fixed slots (for rentals).
- Extended field types (long text, multi-select, file, phone with country code, date range).
- Conditional logic in forms.
- Full schema versioning.

**Stage 3. Monetization:**
- Online payments (Stripe or local provider).
- Deposits and prepayments.
- Cancellation policy with automatic penalty calculation.
- Promo codes and discounts.
- Platform commission.

**Stage 4. Maturity:**
- Roles and teams within a tenant.
- Extended notifications (SMS, push).
- Reviews and ratings.
- Waitlist.
- Configuration templates (template marketplace).
- Analytics and reports for tenants.
- Multi-language support.

**Stage 5. Scaling:**
- Mobile app for clients.
- Public tenant catalog / marketplace.
- API for integrations (CRM, accounting).
- Multi-currency support.

---

## Appendix. Core Rules That Must Never Be Violated

These are load-bearing rules — the entire universality of the platform rests on them. Violating any of them at any phase creates technical debt that is very costly to fix later.

1. **The server is the single source of truth.** All client-side validation must be duplicated on the server.
2. **A booking stores a snapshot of values at the time of creation.** It is never recalculated when the tenant's schema changes.
3. **Tenants never write code or arbitrary regex.** They can only choose from a set of rules with parameters.
4. **Time zones — stored as UTC in the DB, displayed based on context.**
5. **Field identifiers are stable.** Labels can change; identifiers cannot.
6. **Double-booking of a single resource instance is impossible by DB design.**
7. **All amounts, policies, and characteristics affecting the parties' rights are frozen in the booking.**
8. **Tenant isolation is a mandatory constraint on every data query.**
9. **Notification delivery targets are decoupled from user accounts.** A channel can point to any address or endpoint; no `tenant_user` account is required.
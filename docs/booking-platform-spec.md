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
- Required/optional flag.
- Default value.
- Validation rules.
- Visibility and required-state conditions.

### Chapter 3. Field Type Catalog

A finite, controlled set. Extended only when there is a genuine need.

**Text**
- Short text (single line)
- Long text (multiline)

**Numeric**
- Integer
- Decimal
- Money (separate type — currency + precision)

**Choice**
- Single selection (radio/dropdown)
- Multiple selection (checkbox/multi-select)
- Boolean (yes/no)

**Date & Time**
- Date
- Time
- Date and time
- Date range (one type, not two fields)
- Time range

**Contact**
- Phone number (with country code)
- Email

**Files & Media**
- File
- Image

**Composite**
- Address (structured)
- Contact (name + phone + email)

**Other**
- URL
- Rating / score

### Chapter 4. System vs. Custom Fields

**System fields** (always present, cannot be deleted):
- Client contact info (name, phone, email).
- Booking time/period.
- Resource/slot.
- Number of participants (where applicable).

**Custom fields** — added by the tenant for their specific domain. This is where the platform's universality shines.

### Chapter 5. Validation

**Formal type-based validation:**
- Text: min/max length, patterns (from a predefined library only — no arbitrary regex).
- Number: min/max value, step.
- Money: currency, precision, min/max.
- Choice: list of allowed values, "other" mode.
- Date: min/max, prohibition of past dates / weekends.
- Date range: min/max duration.
- Phone: format, allowed countries.
- Email: format, optional domain check.
- File: max size, allowed extensions, max count.

**Cross-field validation:**
- Check-out date must be after check-in date.
- Number of guests ≤ capacity.
- "Pets: yes" → the "type of pet" field becomes required.

**Business validation (server-only):**
- Slot is still free at the time of submission.
- Minimum lead time before start is not violated.
- Client's active booking limit is not exceeded.
- Promo code is valid.

**Validation trigger levels:**
- On input — immediate feedback (format errors).
- On field blur — heavier checks.
- On submit — cross-field validation.
- On server (always) — repeated formal + business validation.
- Asynchronous — slot availability, promo code check.

**Error messages:**
- User-friendly, not technical.
- Localized.
- Tenants can override the default message.

### Chapter 6. Conditional Logic

**What can be controlled conditionally:**
- Field visibility.
- Required state.
- Editability.
- Default value.
- Options list in a select.
- Validation rules.

**Structure of a single condition:**
source field + operator + comparison value.

**Operators by field type:**
- Text: equals, does not equal, contains, is empty, is not empty.
- Number/money: equals, greater than, less than, in range.
- Date: before, after, in range, today / future / past.
- Boolean: true / false.
- Choice: equals, is in list.
- Multi-select: contains all, contains at least one.

**Combining conditions:** AND, OR, NOT, groups with parentheses. Limit nesting depth (2–3 levels); otherwise tenants build labyrinths.

**Section visibility:** a condition applies to a group of fields, not to each one individually.

**What to do with a hidden field's value:**
- Recommended policy: **clear the value on hide**.
- A hidden field **is not validated**, even if it is required.

**Role-based visibility** — a separate axis:
- Clients see certain fields.
- Tenant staff — additionally see internal fields.
- Platform admin — sees everything.

This is not conditional logic; it operates independently.

**Dynamic select options:** the list of values depends on another field (city depends on country, trainer depends on workout type). Implemented as "options = function of another field's value."

**Pitfalls:**
- Circular dependencies (A ↔ B) — must be prohibited at schema creation time.
- The condition builder must have a visual UI.
- Performance of recalculation on changes.
- A form preview mode for the tenant is mandatory.

### Chapter 7. Schema Versioning

**What is versioned (contracts):**
- Booking form schema.
- Pricing rules.
- Cancellation policy.
- Availability rules (with caveats).

**What is not versioned (just an audit log):**
- Resource's marketing description, photos, name.

**The unit of a version is the entire schema as a whole**, not individual fields.

**Version lifecycle:**
- Draft — being edited, not active.
- Published — the current active version.
- Superseded — replaced by a newer version, but still needed to display old bookings.
- Optional: scheduled (becomes active from date X).

**A Published version is immutable.** Any change creates a new version.

**Additive (non-breaking) changes:**
- Adding an optional field.
- Adding an option to a select.
- Relaxing a validation rule.
- Changing a label, reordering fields.
- Making a required field optional.

**Breaking changes:**
- Deleting a field.
- Renaming an identifier (prohibited).
- Changing a field's type.
- Adding a required field.
- Removing a select option.
- Tightening a validation rule.

**Linking a booking to a version:**
A booking stores a reference to the schema + version number + field values. A booking **never migrates automatically** to a new version.

**Reading old bookings:**
1. Take the schema version it was created under.
2. Display it according to the rules of that version.
3. Old fields that were removed in a newer version are still shown.
4. New fields are shown as "no data."

**Data migration strategies:**
- The main rule — **do not migrate**.
- Store data in the format of the version it was created under.
- For analytics — a separate mapping layer to the current representation.

**Tenant experience in the editor:**
- Draft mode.
- Preview.
- Diff against the current version.
- Warnings about breaking changes.
- Version history.
- Rollback.
- Scheduled publishing.

**What else is frozen in a booking:**
- Price at the time of creation.
- Cancellation policy at the time of creation.
- Key resource characteristics (for legal clarity).

General principle: everything that affects the rights/obligations of the parties at the time of booking is frozen in the booking.

---

## Part III. MVP Plan

### Chapter 8. MVP Goals and Constraints

**MVP goal** — validate the hypothesis: can a single engine serve at least 2 different domains (e.g. rentals + fitness classes), with tenants configuring their own forms and schedules?

**What is in scope for MVP:**
- One resource type per tenant.
- Basic field types: text, number, date, single-select, checkbox.
- Default system fields (name, phone, email, slot selection).
- Fixed time slots (not arbitrary ranges).
- Simple weekly working hours + blocking of specific dates.
- Booking confirmation: automatic or manual (tenant's choice).
- Statuses: pending, confirmed, cancelled, completed.
- Email notifications (minimal set).
- One owner per tenant (no teams).
- Public tenant page accessible via slug.

**What is NOT in MVP (deferred):**
- Multiple resource types per tenant.
- Conditional logic in forms.
- Schema versioning (simplified: changes apply immediately, but old bookings retain a snapshot of their values).
- Complex pricing rules, deposits.
- Online payments.
- Cancellation policy with rules.
- Multi-language support.
- Waitlist, overbooking.
- Teams/roles within a tenant.
- Reviews.
- Mobile app.
- Advanced analytics.

**MVP success criteria:**
- 2 pilot tenants from different domains can accept real bookings.
- A client completes the flow "select slot → fill form → confirmation" without errors.
- No cases of double booking.
- Tenant can view and manage their bookings via the dashboard.

### Chapter 9. Architectural Decisions (Summary)

**Multi-tenancy strategy:**
Recommendation for MVP — **shared database, shared schema with tenant_id**. The simplest approach, easy to scale and maintain, cheapest at the start. Migration to separate schemas or databases is possible later for large clients.

**Authentication:**
Two independent contexts — tenant user (logging into the admin panel) and client (logging in to view their own bookings, optional). Guest bookings without registration are supported.

**Storing booking values:**
Structure: `(field_id, value)` pairs + reference to schema. Preserves historical data.

**Time zones:**
Always store in UTC. Display in the resource's time zone for the tenant, and in the client's (or resource's) time zone for the client.

**Double-booking protection:**
Use the database layer: a unique index or a locking transaction on booking creation for a specific (resource, slot) pair.

### Chapter 10. MVP Phases and Tasks

Estimated timeline for a single full team (1–2 developers + designer). Numbers are approximate.

---

#### Phase 0. Foundation (1–2 weeks)

Infrastructure setup and basic entities.

Tasks:
- [ ] Finalize the technology stack (frontend, backend, DB, queue, email provider).
- [ ] Create the repository, set up CI/CD pipeline (lint, tests, deploy).
- [ ] Define the ER model for core entities: tenant, resource, schedule, slot, booking, schema, schema_field.
- [ ] Implement DB migrations.
- [ ] Set up authentication for tenant users (email + password).
- [ ] Basic role system (only "owner" for tenant for now).
- [ ] Logging and basic error monitoring.
- [ ] Connect email notifications sandbox.

---

#### Phase 1. Tenant and Resource (1 week)

Tenant can register and create their first resource.

Tasks:
- [ ] Tenant registration (form + email confirmation).
- [ ] Tenant profile page: name, timezone, contacts, slug.
- [ ] CRUD for a single resource: name, description, photos (1–3).
- [ ] Resource slug (generation and editing).
- [ ] Basic validations on the resource form.

---

#### Phase 2. Form Builder (2 weeks)

Tenant configures what data is collected from the client.

Tasks:
- [ ] Form schema and field model in the DB.
- [ ] Editor UI: field list, add/remove/reorder.
- [ ] Support for 5 field types: short_text, number, date, single_select, checkbox.
- [ ] Field properties: label, identifier, required flag, hint.
- [ ] Type-based validation (min/max length for text, min/max for number, list of values for select).
- [ ] System fields added automatically (name, phone, email, slot selection).
- [ ] Form preview — tenant sees what the client will see.
- [ ] Schema saving (no versioning in MVP — in-place update, but a snapshot of values is taken when a booking is created).
- [ ] Warning to tenant: "if you edit the form, new bookings will have the new structure; existing ones remain as-is."

---

#### Phase 3. Schedule and Availability (1–2 weeks)

Tenant configures when the resource is available.

Tasks:
- [ ] Working hours configuration by day of week (e.g. Mon–Fri 9:00–18:00).
- [ ] Duration of a single slot (15/30/60 minutes or custom).
- [ ] Buffer between slots (optional).
- [ ] Blocking specific dates (vacation, holidays, maintenance).
- [ ] Generation of available slots for the next N days.
- [ ] API "get free slots for a date range" — accounts for existing bookings and blocked dates.
- [ ] Minimum lead time before a booking starts (cannot book for "right now").
- [ ] Maximum booking horizon (cannot book a year in advance).

---

#### Phase 4. Client Booking Flow (2 weeks)

Client lands on the public page and creates a booking.

Tasks:
- [ ] Public routing by tenant/resource slug.
- [ ] Resource page: description, photos, tenant info.
- [ ] Calendar of available slots (from the Phase 3 API).
- [ ] Slot selection.
- [ ] Dynamic form based on tenant schema (system + custom fields).
- [ ] Client-side form validation.
- [ ] Server-side validation (formal + business: slot is free, minimum lead time not violated).
- [ ] Double-booking protection (transaction or unique index).
- [ ] Saving values as (field_id, value) pairs + schema reference.
- [ ] Success page with booking number.
- [ ] Confirmation email to client.
- [ ] Notification email to tenant about a new booking.

---

#### Phase 5. Booking Management (1–2 weeks)

Tenant views and processes bookings in their admin panel.

Tasks:
- [ ] Tenant dashboard — booking list.
- [ ] Filters: status, date, search by name/phone.
- [ ] Booking card — all client data + action history.
- [ ] Booking actions: confirm, cancel, mark as completed, mark as no-show.
- [ ] Tenant setting: auto-confirm or manual confirmation.
- [ ] Email to client on status change.
- [ ] Reminder to client 24 hours / 1 hour before the booking (cron job).
- [ ] Audit history of actions (who, when, which status was changed).

---

#### Phase 6. Polish and Launch (1 week)

Clean up and launch with pilot tenants.

Tasks:
- [ ] Testing edge cases: race conditions on booking creation, attempt to book a past time slot, attempt to cancel an already completed booking.
- [ ] Data isolation audit — tenant A must never see tenant B's data under any circumstances.
- [ ] Basic styling of the public page and admin panel.
- [ ] Tenant documentation (how to configure a resource, form, and schedule).
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

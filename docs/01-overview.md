# Slot — Generic Booking Platform

## What is Slot?

Slot is a domain-agnostic booking platform built with ASP.NET Core and PostgreSQL.
It provides a generic model for scheduling, resource management, customer bookings,
and credit-based memberships — configurable to any business type.

## Example domains

| Domain         | Resource (Staff) | Resource (Space) | ServiceType  | Slot                   |
| -------------- | ---------------- | ---------------- | ------------ | ---------------------- |
| Fitness studio | Trainer          | —                | Yoga class   | Group session          |
| Barbershop     | Barber           | —                | Haircut      | Individual appointment |
| Medical clinic | Doctor           | Examination room | Consultation | Appointment            |
| Coworking      | —                | Meeting room     | Room rental  | Time block             |
| Tutoring       | Tutor            | —                | Math lesson  | Individual session     |

## Tech stack

- **Runtime:** .NET 10
- **Framework:** ASP.NET Core Web API
- **ORM:** Entity Framework Core
- **Relational database:** PostgreSQL (entities, relationships, transactional data)
- **NoSQL database:** MongoDB (tenant config documents, resource metadata)
- **Architecture:** Clean Architecture (Domain / Application / Infrastructure / API)

## Documents

| File                        | Description                           |
| --------------------------- | ------------------------------------- |
| `02-domain-model.md`        | Entities, enums, relationships        |
| `03-tenant-config.md`       | How tenants configure their domain    |
| `04-business-rules.md`      | Booking, cancellation, credit rules   |
| `05-api-contract.md`        | Endpoints and request/response shapes |
| `06-project-structure.md`   | .NET solution layout                  |
| `07-implementation-plan.md` | Step-by-step build plan               |

## Versioning roadmap

| Version | Scope                                                    |
| ------- | -------------------------------------------------------- |
| v1      | Core booking engine, NoSQL-backed tenant config          |
| v2      | Admin API for runtime configuration                      |
| v3      | Pre-built domain templates (fitness, barbershop, clinic) |

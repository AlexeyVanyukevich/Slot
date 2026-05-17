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

| File                          | Description                                         |
| ----------------------------- | --------------------------------------------------- |
| `02-implementation-plan.md`   | Phase index — links to each phase document          |
| `phase-1-domain.md`           | Domain model: entities, enums, relationships, rules |
| `phase-2-persistence.md`      | PostgreSQL / EF Core mapping and repositories       |
| `phase-3-config.md`           | MongoDB tenant config and caching                   |
| `phase-4-application.md`      | Use cases and business orchestration                |
| `phase-5-api.md`              | HTTP endpoints, middleware, validation              |

## Versioning roadmap

| Version | Scope                                                    |
| ------- | -------------------------------------------------------- |
| v1      | Core booking engine, NoSQL-backed tenant config          |
| v2      | Admin API for runtime configuration                      |
| v3      | Pre-built domain templates (fitness, barbershop, clinic) |

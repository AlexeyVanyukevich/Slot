# SlotWise — Project Structure

## Solution layout

```
SlotWise.sln
├── src/
│   ├── SlotWise.Domain/
│   ├── SlotWise.Application/
│   ├── SlotWise.Infrastructure/
│   └── SlotWise.Api/
└── tests/
    ├── SlotWise.Domain.Tests/
    └── SlotWise.Application.Tests/
```

---

## SlotWise.Domain
Pure domain layer — no dependencies on infrastructure or frameworks.

```
SlotWise.Domain/
├── Entities/
│   ├── Tenant.cs
│   ├── Customer.cs
│   ├── Resource.cs
│   ├── ServiceType.cs
│   ├── Slot.cs
│   ├── Booking.cs
│   └── CreditPack.cs
├── Enums/
│   ├── CustomerStatus.cs
│   ├── ResourceType.cs
│   ├── SlotStatus.cs
│   └── BookingStatus.cs
├── Exceptions/
│   ├── DomainException.cs
│   ├── SlotFullException.cs
│   ├── BookingConflictException.cs
│   ├── CancellationDeadlineException.cs
│   └── InsufficientCreditsException.cs
└── ValueObjects/
    └── TenantConfig.cs       # Deserialized from ConfigJson
```

---

## SlotWise.Application
Use cases, service interfaces, DTOs. Depends only on Domain.

```
SlotWise.Application/
├── Bookings/
│   ├── CreateBookingCommand.cs
│   ├── CreateBookingHandler.cs
│   ├── CancelBookingCommand.cs
│   └── CancelBookingHandler.cs
├── Slots/
│   ├── CreateSlotCommand.cs
│   ├── CompleteSlotCommand.cs
│   └── CancelSlotCommand.cs
├── CreditPacks/
│   ├── AddCreditPackCommand.cs
│   ├── FreezePackCommand.cs
│   └── UnfreezePackCommand.cs
├── Interfaces/
│   ├── IBookingRepository.cs
│   ├── ISlotRepository.cs
│   ├── ICustomerRepository.cs
│   ├── ICreditPackRepository.cs
│   └── ITenantConfigProvider.cs
└── Common/
    └── IUnitOfWork.cs
```

---

## SlotWise.Infrastructure
EF Core, repositories, config loading. Depends on Application.

```
SlotWise.Infrastructure/
├── Persistence/
│   ├── SlotWiseDbContext.cs
│   ├── Migrations/
│   └── Configurations/          # EF entity type configurations
│       ├── TenantConfiguration.cs
│       ├── CustomerConfiguration.cs
│       ├── SlotConfiguration.cs
│       ├── BookingConfiguration.cs
│       └── CreditPackConfiguration.cs
├── Repositories/
│   ├── BookingRepository.cs
│   ├── SlotRepository.cs
│   └── CustomerRepository.cs
└── Config/
    └── TenantConfigProvider.cs  # Reads + deserializes ConfigJson
```

---

## SlotWise.Api
HTTP layer — controllers, middleware, DI setup.

```
SlotWise.Api/
├── Controllers/
│   ├── TenantsController.cs
│   ├── CustomersController.cs
│   ├── ResourcesController.cs
│   ├── ServiceTypesController.cs
│   ├── SlotsController.cs
│   ├── BookingsController.cs
│   └── CreditPacksController.cs
├── Middleware/
│   ├── TenantResolutionMiddleware.cs   # Reads X-Tenant-Id header
│   └── ExceptionHandlingMiddleware.cs  # Maps DomainException → ProblemDetails
├── DTOs/
│   ├── Requests/
│   └── Responses/
└── Program.cs
```

---

## Key design decisions

**TenantId propagation** — resolved once in middleware and stored in a scoped
`ITenantContext` service. All repositories filter by `TenantId` automatically
through a base query filter in `DbContext`.

**TenantConfig** — deserialized from `Tenant.ConfigJson` into a `TenantConfig`
value object and injected via `ITenantConfigProvider`. Business rules read from
it instead of hardcoded values.

**No MediatR (v1)** — commands and handlers are wired directly to keep the
project lean. MediatR can be introduced later if the number of use cases grows.

# SlotWise — API Contract

Base URL: `/api/v1`

Tenant is identified via request header: `X-Tenant-Id: {tenantId}`

All error responses follow RFC 7807 `ProblemDetails`:
```json
{
  "type": "https://slotwise.dev/errors/slot-full",
  "title": "Slot is fully booked",
  "status": 409,
  "detail": "No available capacity for slot 3fa85f64"
}
```

---

## Tenants

| Method | URL | Description |
|---|---|---|
| POST | /tenants | Register a new tenant |
| GET | /tenants/{id} | Get tenant details |
| GET | /tenants/{id}/config | Get tenant configuration |
| PUT | /tenants/{id}/config | Replace tenant configuration |

### POST /tenants
```json
{
  "name": "My Fitness Studio",
  "slug": "my-fitness-studio"
}
```

---

## Customers

| Method | URL | Description |
|---|---|---|
| GET | /customers | List customers (paginated) |
| GET | /customers/{id} | Get customer by ID |
| POST | /customers | Create a customer |
| PATCH | /customers/{id}/status | Update customer status |

### POST /customers
```json
{
  "firstName": "Anna",
  "lastName": "Smith",
  "phone": "+48123456789",
  "email": "anna@example.com"
}
```

### PATCH /customers/{id}/status
```json
{ "status": "Frozen" }
```

---

## Resources

| Method | URL | Description |
|---|---|---|
| GET | /resources | List resources |
| POST | /resources | Create a resource |
| PATCH | /resources/{id} | Update resource |
| DELETE | /resources/{id} | Deactivate resource |

### POST /resources
```json
{
  "name": "Anna (Yoga)",
  "type": "Staff",
  "metadata": { "specialization": "Yoga, Pilates" }
}
```

---

## Service Types

| Method | URL | Description |
|---|---|---|
| GET | /service-types | List service types |
| POST | /service-types | Create a service type |
| PATCH | /service-types/{id} | Update service type |

### POST /service-types
```json
{
  "name": "Yoga for Beginners",
  "durationMinutes": 60,
  "capacity": 12,
  "requiresResource": true,
  "creditCost": 1,
  "description": "Gentle practice for newcomers"
}
```

---

## Slots (Schedule)

| Method | URL | Description |
|---|---|---|
| GET | /slots | List slots with filters |
| GET | /slots/{id} | Get slot by ID |
| POST | /slots | Create a slot |
| DELETE | /slots/{id} | Cancel a slot |
| POST | /slots/{id}/complete | Mark slot as completed |

### GET /slots (query params)
```
?from=2026-04-21&to=2026-04-28&resourceId={guid}&serviceTypeId={guid}&status=Scheduled
```

### POST /slots
```json
{
  "serviceTypeId": "...",
  "resourceId": "...",
  "startsAt": "2026-04-25T10:00:00"
}
```

### DELETE /slots/{id}
```json
{ "reason": "Trainer is unavailable" }
```

---

## Bookings

| Method | URL | Description |
|---|---|---|
| GET | /bookings | List bookings with filters |
| GET | /customers/{id}/bookings | Customer's booking history |
| POST | /bookings | Create a booking |
| DELETE | /bookings/{id} | Cancel a booking |
| PATCH | /bookings/{id}/status | Mark as Attended or NoShow |

### POST /bookings
```json
{
  "customerId": "...",
  "slotId": "...",
  "creditPackId": "..."
}
```

### PATCH /bookings/{id}/status
```json
{ "status": "Attended" }
```

---

## Credit Packs

| Method | URL | Description |
|---|---|---|
| GET | /customers/{id}/credit-packs | List customer's packs |
| POST | /customers/{id}/credit-packs | Add a new pack |
| POST | /credit-packs/{id}/freeze | Freeze a pack |
| POST | /credit-packs/{id}/unfreeze | Unfreeze a pack |

### POST /customers/{id}/credit-packs
```json
{
  "totalCredits": 10,
  "validFrom": "2026-04-21",
  "validUntil": "2026-07-21"
}
```

---

## HTTP status codes

| Code | When |
|---|---|
| 200 | Success |
| 201 | Resource created |
| 400 | Invalid request or business rule violation |
| 404 | Resource not found |
| 409 | Conflict (duplicate booking, slot full) |
| 422 | Validation error |

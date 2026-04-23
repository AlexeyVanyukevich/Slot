# SlotWise — Business Rules

All rules marked with ⚙️ are controlled by tenant configuration.

---

## Booking a slot

- A slot must have `Status = Scheduled`
- The slot's `StartsAt` must be in the future
- The customer must not already have a `Confirmed` booking for the same slot
- The number of `Confirmed` bookings must be less than `ServiceType.Capacity`
- If `creditPack.enabled = true`, the customer must have an active `CreditPack`:
  - `ValidUntil >= today`
  - `IsFrozen = false`
  - `RemainingCredits >= ServiceType.CreditCost`

---

## Cancelling a booking (by customer)

- ⚙️ Only allowed if `booking.allowCustomerCancellation = true`
- If cancelled before the deadline (`StartsAt - cancellationDeadlineHours`):
  - Status → `CancelledByCustomer`
  - Credits are returned to the pack
- If cancelled after the deadline:
  - Status → `CancelledByCustomer`
  - ⚙️ Credits returned only if `booking.creditReturnOnLateCancellation = true`

---

## Cancelling a booking (by tenant)

- Can be done at any time, regardless of deadline
- Status → `CancelledByTenant`
- Credits are always returned to the customer's pack

---

## Cancelling a slot (by tenant)

- The slot `Status` → `Cancelled`, `CancelReason` is recorded
- All `Confirmed` bookings → `CancelledByTenant`
- Credits are returned to all affected customers automatically

---

## Completing a slot

- Only possible if `StartsAt` has already passed
- Slot `Status` → `Completed`
- Each `Confirmed` booking must be manually marked as `Attended` or `NoShow`
- Credits are finally consumed on `Attended`
- Credits are forfeited on `NoShow` (not returned)

---

## Credit pack rules

- Credits are **reserved** (not yet consumed) when a booking is confirmed
- Credits are **consumed** when booking status becomes `Attended`
- Credits are **released** back to the pack on cancellation (subject to rules above)
- A pack cannot be used if:
  - `ValidUntil < today` — expired
  - `IsFrozen = true` — frozen
  - `RemainingCredits < ServiceType.CreditCost` — insufficient credits
- ⚙️ When frozen: if `creditPack.expiryExtendedOnFreeze = true`,
  `ValidUntil` is extended by the number of days the pack was frozen when unfreezing

---

## Constraints summary

| Rule | Configurable | Default |
|---|---|---|
| Customer can cancel | ✅ | true |
| Cancellation deadline | ✅ | 2 hours |
| Credits returned on late cancel | ✅ | false |
| Credits returned on tenant cancel | ❌ | always |
| Credits consumed on NoShow | ❌ | always |
| Freeze extends expiry | ✅ | true |

using Microsoft.EntityFrameworkCore.Diagnostics;

using Slot.Domain.Interfaces;

namespace Slot.Persistence.Interceptors;

internal sealed class AuditableInterceptor(TimeProvider? timeProvider = null) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = eventData.Context.ChangeTracker.Entries<IAuditable>().Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added).ToList();

        if (entries.Count == 0)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        var now = (timeProvider ?? TimeProvider.System).GetUtcNow();

        foreach (var entry in entries)
        {
            entry.Property(e => e.CreatedAt).CurrentValue = now;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

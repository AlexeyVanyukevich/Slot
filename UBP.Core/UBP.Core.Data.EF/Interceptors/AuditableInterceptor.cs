using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using UBP.Core.Interfaces;

namespace UBP.Core.Data.EF.Interceptors;

internal sealed class AuditableInterceptor(TimeProvider? timeProvider = null) : SaveChangesInterceptor
{
    readonly static List<EntityState> interestedEntityStates = [EntityState.Added, EntityState.Modified];
    TimeProvider TimeProvider => timeProvider ?? TimeProvider.System;
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = eventData.Context.ChangeTracker.Entries<IAuditable>().Where(e => interestedEntityStates.Contains(e.State)).ToList();

        if (entries.Count == 0)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        var now = TimeProvider.GetUtcNow();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedAt).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

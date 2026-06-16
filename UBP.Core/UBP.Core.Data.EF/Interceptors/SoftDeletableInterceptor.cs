using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using UBP.Core.Interfaces;

namespace UBP.Core.Data.EF.Interceptors;

internal sealed class SoftDeletableInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var enrties = eventData.Context.ChangeTracker.Entries<ISoftDeletable>().Where(e => e.State == EntityState.Deleted);

        foreach (var entry in enrties)
        {
            entry.Property(e => e.IsDeleted).CurrentValue = true;
            entry.State = EntityState.Modified;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}


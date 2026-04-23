using Microsoft.EntityFrameworkCore.Diagnostics;

using Slot.Domain.Interfaces;

namespace Slot.Persistence.Interceptors;

internal sealed class AuditableInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entities = eventData.Context.ChangeTracker.Entries<IAuditable>().Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added);

        if (entities.Any())
        {
            var now = timeProvider.GetUtcNow();

            foreach (var entity in entities)
            {
                entity.Property(e => e.CreatedAt).CurrentValue = now;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

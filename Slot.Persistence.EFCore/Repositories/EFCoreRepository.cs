using Microsoft.EntityFrameworkCore;

using Slot.Persistence.Abstractions.Interfaces;
using Slot.Persistence.Abstractions.Repositories;

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Slot.Persistence.EFCore.Repositories;

public abstract class EFCoreRepository<TEntity, TKey>(IDbContext dbContext) : Repository<TEntity, TKey>(dbContext) where TEntity : class
{
    public override Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
    {
        return GetExpressionQuery(predicate, tracking).ToListAsync(cancellationToken);
    }

    public override Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
    {
        return GetExpressionQuery(predicate, tracking).FirstOrDefaultAsync(cancellationToken);
    }
}

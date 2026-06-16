using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

using UBP.Core.Persistence.Interfaces;
using UBP.Core.Persistence.Repositories;

namespace UBP.Core.Persistence.EF.Repositories;

public abstract class EFRepository<TEntity>(IDbContext dbContext) : Repository<TEntity>(dbContext) where TEntity : class
{
    public override Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return GetExpressionQuery(predicate).ToListAsync(cancellationToken);
    }

    public override Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return GetExpressionQuery(predicate).FirstOrDefaultAsync(cancellationToken);
    }

}

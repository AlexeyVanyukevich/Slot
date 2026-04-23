using Microsoft.EntityFrameworkCore;

using Slot.Persistence.Interfaces;

using System.Linq.Expressions;

namespace Slot.Persistence.Repositories;

internal abstract class Repository<TEntity, TKey>(DbContext dbContext) : IRepository<TEntity, TKey> where TEntity : class
{
    protected DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();
    public void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public void Delete(TKey id)
    {
        var entity = FromId(id);
        DbSet.Attach(entity);
        DbSet.Remove(entity);
    }

    public Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
    {
        return GetExpressionQuery(predicate, tracking).ToListAsync(cancellationToken);
    }

    public Task<TEntity?> GetByIdAsync(TKey id, bool tracking = false, CancellationToken cancellationToken = default)
    {
        return GetSingleAsync(CreateIdExpression(id), tracking, cancellationToken);
    }

    public Task<TEntity?> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
    {
        return GetExpressionQuery(predicate, tracking).FirstOrDefaultAsync(cancellationToken);
    }


    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }
    private IQueryable<TEntity> GetExpressionQuery(Expression<Func<TEntity, bool>> predicate, bool tracking)
    {
        return Query(tracking).Where(predicate);
    }

    protected virtual IQueryable<TEntity> Query(bool tracking)
    {
        return tracking ? DbSet : DbSet.AsNoTracking();
    }

    protected abstract TEntity FromId(TKey id);

    protected abstract Expression<Func<TEntity, bool>> CreateIdExpression(TKey id);
}

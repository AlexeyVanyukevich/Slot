using Slot.Persistence.Abstractions.Interfaces;

using System.Linq.Expressions;

namespace Slot.Persistence.Abstractions.Repositories;
public abstract class Repository<TEntity, TKey>(IDbContext dbContext) : IRepository<TEntity, TKey> where TEntity : class
{
    protected IDbSet<TEntity> DbSet { get; } = dbContext.DbSet<TEntity>();
    public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Add(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = FromId(id);
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
    {
        List<TEntity> entities = GetExpressionQuery(predicate, tracking).ToList();

        return Task.FromResult(entities);
    }

    public virtual Task<TEntity?> GetByIdOrDefaultAsync(TKey id, bool tracking = false, CancellationToken cancellationToken = default)
    {
        return GetSingleOrDefaultAsync(CreateIdExpression(id), tracking, cancellationToken);
    }

    public virtual Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
    {
        TEntity? entity = GetExpressionQuery(predicate, tracking).FirstOrDefault();

        return Task.FromResult(entity);
    }


    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }
    protected IQueryable<TEntity> GetExpressionQuery(Expression<Func<TEntity, bool>> predicate, bool tracking)
    {
        return Query(tracking).Where(predicate);
    }

    protected virtual IQueryable<TEntity> Query(bool tracking)
    {
        return tracking ? DbSet.AsQueryable() : DbSet.AsNoTracking();
    }

    protected abstract TEntity FromId(TKey id);

    protected abstract Expression<Func<TEntity, bool>> CreateIdExpression(TKey id);
}

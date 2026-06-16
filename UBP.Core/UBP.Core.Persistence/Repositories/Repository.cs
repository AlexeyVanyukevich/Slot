using UBP.Core.Persistence.Interfaces;

using System.Linq.Expressions;

namespace UBP.Core.Persistence.Repositories;
public class Repository<TEntity>(IDbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    protected IDbSet<TEntity> DbSet { get; } = dbContext.DbSet<TEntity>();
    public virtual void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public virtual Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        List<TEntity> entities = GetExpressionQuery(predicate).ToList();

        return Task.FromResult(entities);
    }

    public virtual Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        TEntity? entity = GetExpressionQuery(predicate).FirstOrDefault();

        return Task.FromResult(entity);
    }


    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }
    protected IQueryable<TEntity> GetExpressionQuery(Expression<Func<TEntity, bool>> predicate)
    {
        return DbSet.AsQueryable().Where(predicate);
    }
}

using Microsoft.EntityFrameworkCore;

using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Persistence.EF.Models;

internal sealed class EFDbSet<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet) : IDbSet<TEntity> where TEntity : class
{
    public void Add(TEntity entity)
    {
        dbSet.Add(entity);
    }

    public IQueryable<TEntity> AsQueryable()
    {
        return dbSet;
    }

    public void Remove(TEntity entity)
    {
        var entry = dbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            dbSet.Attach(entity);
        }
        dbSet.Remove(entity);
    }

    public void Update(TEntity entity)
    {
        dbSet.Update(entity);
    }
}

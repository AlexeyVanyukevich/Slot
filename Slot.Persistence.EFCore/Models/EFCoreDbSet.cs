using Microsoft.EntityFrameworkCore;

using Slot.Persistence.Abstractions.Interfaces;

namespace Slot.Persistence.Models;

internal sealed class EFCoreDbSet<TEntity>(DbSet<TEntity> dbSet) : IDbSet<TEntity> where TEntity : class
{
    public void Add(TEntity entity)
    {
        dbSet.Add(entity);
    }

    public IQueryable<TEntity> AsNoTracking()
    {
        return dbSet.AsNoTracking();
    }

    public IQueryable<TEntity> AsQueryable()
    {
        return dbSet;
    }

    public void Remove(TEntity entity)
    {
        dbSet.Attach(entity);
        dbSet.Remove(entity);
    }

    public void Update(TEntity entity)
    {
        dbSet.Update(entity);
    }
}

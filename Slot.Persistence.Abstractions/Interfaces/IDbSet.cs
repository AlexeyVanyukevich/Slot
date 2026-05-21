namespace Slot.Persistence.Abstractions.Interfaces;

public interface IDbSet<TEntity>
{
    void Add(TEntity entity);
    void Remove(TEntity entity);
    void Update(TEntity entity);

    IQueryable<TEntity> AsNoTracking();

    IQueryable<TEntity> AsQueryable();
}

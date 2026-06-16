namespace UBP.Core.Persistence.Interfaces;

public interface IDbSet<TEntity>
{
    void Add(TEntity entity);
    void Remove(TEntity entity);
    void Update(TEntity entity);
    IQueryable<TEntity> AsQueryable();
}

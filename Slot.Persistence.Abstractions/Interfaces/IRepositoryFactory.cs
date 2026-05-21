namespace Slot.Persistence.Abstractions.Interfaces;

public interface IRepositoryFactory
{
    IRepository<TEntity, TKey> Create<TEntity, TKey>(IDbContext appDbContext) where TEntity : class;
}

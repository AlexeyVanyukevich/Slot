namespace Slot.Persistence.Abstractions.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class;
}

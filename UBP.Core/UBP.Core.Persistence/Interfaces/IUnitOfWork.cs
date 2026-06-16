namespace UBP.Core.Persistence.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
}

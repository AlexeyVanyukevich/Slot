using Slot.Persistence.Abstractions.Interfaces;

namespace Slot.Persistence.Abstractions;

public class UnitOfWork(IDbContext dbContext, IRepositoryFactory repositoryFactory) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];
    protected IRepositoryFactory RepositoryFactory => repositoryFactory;
    protected IDbContext DbContext => dbContext;

    public IRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class
    {
        if (_repositories.TryGetValue(typeof(TEntity), out var cached) && cached is IRepository<TEntity, TKey> existingRepository)
            return existingRepository;

        var repository = repositoryFactory.Create<TEntity, TKey>(dbContext);
        _repositories[typeof(TEntity)] = repository;
        return repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }


}


using UBP.Core.Persistence.Interfaces;
using UBP.Core.Persistence.Repositories;

namespace UBP.Core.Persistence.Factories;

public class RepositoryFactory : IRepositoryFactory
{
    public virtual IRepository<TEntity> Create<TEntity>(IDbContext dbContext) where TEntity : class
    {
        return new Repository<TEntity>(dbContext);
    }
}

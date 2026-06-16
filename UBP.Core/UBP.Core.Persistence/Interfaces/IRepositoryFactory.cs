namespace UBP.Core.Persistence.Interfaces;

public interface IRepositoryFactory
{
    IRepository<TEntity> Create<TEntity>(IDbContext dbContext) where TEntity : class;
}

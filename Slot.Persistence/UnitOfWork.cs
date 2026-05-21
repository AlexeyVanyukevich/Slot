using Slot.Domain.Entities;
using Slot.Persistence.Abstractions.Interfaces;
using Slot.Persistence.Contexts;


namespace Slot.Persistence;

internal sealed class UnitOfWork(AppDbContext appDbContext, IRepositoryFactory repositoryFactory) : Abstractions.UnitOfWork(appDbContext, repositoryFactory), Interfaces.IUnitOfWork
{
    public IRepository<TEntity, int> Repository<TEntity>() where TEntity : Entity
    {
        return RepositoryFactory.Create<TEntity, int>(DbContext);
    }
}

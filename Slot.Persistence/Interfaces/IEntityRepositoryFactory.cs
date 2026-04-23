using Slot.Domain.Entities;

namespace Slot.Persistence.Interfaces;

public interface IEntityRepositoryFactory
{
    IRepository<TEntity, int> Create<TEntity>() where TEntity : Entity, new();
}

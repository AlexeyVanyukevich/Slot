using Slot.Domain.Entities;
using Slot.Persistence.Abstractions.Interfaces;

namespace Slot.Persistence.Interfaces;

public interface IUnitOfWork : Abstractions.Interfaces.IUnitOfWork
{
    IRepository<TEntity, int> Repository<TEntity>() where TEntity : Entity;
}

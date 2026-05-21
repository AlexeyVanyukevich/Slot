using Slot.Domain.Entities;
using Slot.Persistence.Abstractions.Interfaces;
using Slot.Persistence.Contexts;
using Slot.Persistence.Repositories;

namespace Slot.Persistence.Factories;

internal sealed class RepositoryFactory : IRepositoryFactory
{
    public IRepository<TEntity, TKey> Create<TEntity, TKey>(IDbContext appDbContext) where TEntity : class
    {
        if (typeof(TKey) == typeof(int)
            && typeof(TEntity).IsAssignableTo(typeof(Entity))
            && typeof(TEntity).GetConstructor(Type.EmptyTypes) is not null
            && appDbContext is AppDbContext efContext)
        {
            var repoType = typeof(EntityRepository<>).MakeGenericType(typeof(TEntity));
            return (IRepository<TEntity, TKey>)Activator.CreateInstance(repoType, efContext)!;
        }

        throw new NotSupportedException(
            $"No repository registered for entity '{typeof(TEntity).Name}' with key '{typeof(TKey).Name}'.");
    }
}

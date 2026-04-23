using Microsoft.Extensions.DependencyInjection;

using Slot.Domain.Entities;
using Slot.Persistence.Interfaces;
using Slot.Persistence.Repositories;

namespace Slot.Persistence.Factories;

internal sealed class EntityRepositoryFactory(IServiceProvider serviceProvider) : IEntityRepositoryFactory
{
    public IRepository<TEntity, int> Create<TEntity>() where TEntity : Entity, new()
    {
        return serviceProvider.GetRequiredService<EntityRepository<TEntity>>();
    }
}

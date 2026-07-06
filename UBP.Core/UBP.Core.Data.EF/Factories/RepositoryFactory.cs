using UBP.Core.Data.EF.Repositories;
using UBP.Core.Entities;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Data.EF.Factories;

internal sealed class RepositoryFactory : Persistence.Factories.RepositoryFactory
{
    public override IRepository<TEntity> Create<TEntity>(IDbContext dbContext) where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (entityType.GetConstructor(Type.EmptyTypes) is not null)
        {
            var entityIdType = GetEntityIdType(entityType)
                ?? throw new InvalidOperationException($"Could not determine the id type for entity '{entityType}'.");

            var repoType = typeof(EntityRepository<,>).MakeGenericType(entityType, entityIdType);
            return (IRepository<TEntity>)Activator.CreateInstance(repoType, dbContext)!;
        }

        return base.Create<TEntity>(dbContext);
    }

    private static Type? GetEntityIdType(Type entityType)
    {
        var type = entityType;
        while (type is not null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Entity<>))
                return type.GetGenericArguments()[0];

            type = type.BaseType;
        }

        return null;
    }
}

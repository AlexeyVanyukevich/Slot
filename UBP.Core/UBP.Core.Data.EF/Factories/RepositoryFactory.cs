using UBP.Core.Data.EF.Repositories;
using UBP.Core.Entities;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Data.EF.Factories;

internal sealed class RepositoryFactory : Persistence.Factories.RepositoryFactory
{
    public override IRepository<TEntity> Create<TEntity>(IDbContext dbContext) where TEntity : class
    {
        if (typeof(TEntity).IsAssignableTo(typeof(Entity)) && typeof(TEntity).GetConstructor(Type.EmptyTypes) is not null)
        {
            var repoType = typeof(EntityRepository<>).MakeGenericType(typeof(TEntity));
            return (IRepository<TEntity>)Activator.CreateInstance(repoType, dbContext)!;
        }

        return base.Create<TEntity>(dbContext);
    }
}

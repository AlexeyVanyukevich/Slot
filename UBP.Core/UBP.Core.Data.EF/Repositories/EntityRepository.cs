using UBP.Core.Data.EF.Interfaces;
using UBP.Core.Entities;
using UBP.Core.Persistence.EF.Repositories;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Data.EF.Repositories;

internal sealed class EntityRepository<TEntity,TKey>(IDbContext dbContext) : EFRepository<TEntity>(dbContext), IEntityRepository<TEntity, TKey> where TEntity : Entity<TKey>, new()
{
    public void Delete(TKey id)
    {
        var entity = new TEntity { Id = id };
        DbSet.Remove(entity);
    }
}

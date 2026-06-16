using UBP.Core.Data.EF.Interfaces;
using UBP.Core.Entities;
using UBP.Core.Persistence.EF.Repositories;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Data.EF.Repositories;

internal sealed class EntityRepository<TEntity>(IDbContext dbContext) : EFRepository<TEntity>(dbContext), IEntityRepository<TEntity> where TEntity : Entity, new()
{
    public void Delete(int id)
    {
        var entity = new TEntity { Id = id };
        DbSet.Remove(entity);
    }
}

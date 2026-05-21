using Slot.Domain.Entities;
using Slot.Persistence.Contexts;
using Slot.Persistence.EFCore.Repositories;

using System.Linq.Expressions;

namespace Slot.Persistence.Repositories;

internal sealed class EntityRepository<TEntity>(AppDbContext appDbContext) : EFCoreRepository<TEntity, int>(appDbContext) where TEntity : Entity, new()
{
    protected override Expression<Func<TEntity, bool>> CreateIdExpression(int id)
    {
        return x => x.Id == id;
    }

    protected override TEntity FromId(int id)
    {
        return new TEntity { Id = id };
    }
}

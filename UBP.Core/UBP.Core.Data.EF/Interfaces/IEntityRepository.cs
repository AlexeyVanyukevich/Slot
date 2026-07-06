using UBP.Core.Entities;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Data.EF.Interfaces;

public interface IEntityRepository<TEntity, TKey> : IRepository<TEntity> where TEntity : Entity<TKey>
{
    void Delete(TKey id);
}

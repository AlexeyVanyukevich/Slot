using UBP.Core.Entities;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Data.EF.Interfaces;

public interface IEntityRepository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    void Delete(int id);
}

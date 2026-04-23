using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Slot.Persistence.Interfaces;

public interface IRepository<TEntity, TKey>
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TKey id);
    Task<TEntity?> GetByIdAsync(TKey id, bool tracking = false, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default);
    Task<TEntity?> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default);
}

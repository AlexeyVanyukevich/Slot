using System.Linq.Expressions;

namespace Slot.Persistence.Abstractions.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdOrDefaultAsync(TKey id, bool tracking = false, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default);
    Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default);
}

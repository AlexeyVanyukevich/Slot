namespace UBP.Core.Persistence.Interfaces;

public interface IDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IDbSet<TEntity> DbSet<TEntity>() where TEntity : class;
}

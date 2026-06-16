using Microsoft.EntityFrameworkCore;

using UBP.Core.Persistence.Interfaces;
using UBP.Core.Persistence.EF.Models;

namespace UBP.Core.Persistence.EF;

public class EFDbContext(DbContextOptions options) : DbContext(options), IDbContext
{

    public IDbSet<TEntity> DbSet<TEntity>() where TEntity : class
    {
        return new EFDbSet<TEntity>(this, Set<TEntity>());
    }
}

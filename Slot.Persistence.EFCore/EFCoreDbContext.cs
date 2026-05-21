using Microsoft.EntityFrameworkCore;

using Slot.Persistence.Abstractions.Interfaces;
using Slot.Persistence.Models;

namespace Slot.Persistence.EFCore;

public class EFCoreDbContext(DbContextOptions options) : DbContext(options), IDbContext
{

    public IDbSet<TEntity> DbSet<TEntity>() where TEntity : class
    {
        return new EFCoreDbSet<TEntity>(Set<TEntity>());
    }
}

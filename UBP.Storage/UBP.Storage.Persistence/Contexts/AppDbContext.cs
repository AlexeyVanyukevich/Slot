using Microsoft.EntityFrameworkCore;

using UBP.Core.Persistence.EF;

namespace UBP.Storage.Persistence.Contexts;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : EFDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

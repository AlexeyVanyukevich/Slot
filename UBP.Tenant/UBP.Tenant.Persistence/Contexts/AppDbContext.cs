using Microsoft.EntityFrameworkCore;

using UBP.Core.Data.EF.Extensions;
using UBP.Core.Persistence.EF;

namespace UBP.Tenant.Persistence.Contexts;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : EFDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplySoftDeleteFilter();
    }

}

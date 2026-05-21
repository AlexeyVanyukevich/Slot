using Microsoft.EntityFrameworkCore;

using Slot.Domain.Interfaces;
using Slot.Persistence.EFCore;

using System.Linq.Expressions;

namespace Slot.Persistence.Contexts;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : EFCoreDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ApplySoftDeleteConfiguration(modelBuilder);
    }

    private static void ApplySoftDeleteConfiguration(ModelBuilder modelBuilder)
    {
        foreach (var clrType in modelBuilder.Model.GetEntityTypes().Where(x => typeof(ISoftDeletable).IsAssignableFrom(x.ClrType)).Select(x => x.ClrType))
        {
            modelBuilder.Entity(clrType).HasQueryFilter(BuildSoftDeleteFilter(clrType));
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type type)
    {
        var param = Expression.Parameter(type);
        var prop = Expression.Property(param, nameof(ISoftDeletable.IsDeleted));
        var body = Expression.Equal(prop, Expression.Constant(false));
        return Expression.Lambda(body, param);
    }
}

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

using UBP.Core.Interfaces;

namespace UBP.Core.Data.EF.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);


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

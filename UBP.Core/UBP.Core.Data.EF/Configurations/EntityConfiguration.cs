using System.Linq.Expressions;

using UBP.Core.Entities;
using UBP.Core.Persistence.EF.Configurations;

namespace UBP.Core.Data.EF.Configurations;

public abstract class EntityConfiguration<TEntity, TKey>(string tableName)
    : EntityTypeConfiguration<TEntity>(tableName) where TEntity : Entity<TKey>
{
    protected override Expression<Func<TEntity, object?>> KeyExpression => x => x.Id;
}


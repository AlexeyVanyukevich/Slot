using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.Linq.Expressions;

namespace UBP.Core.Persistence.EF.Configurations;

public abstract class EntityTypeConfiguration<TEntity>(string tableName) : IEntityTypeConfiguration<TEntity> where TEntity : class
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        if (builder is null)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
        builder.HasKey(KeyExpression);
        builder.ToTable(tableName);
    }

    protected abstract Expression<Func<TEntity, object?>> KeyExpression { get; }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Slot.Domain.Entities;

namespace Slot.Persistence.Configurations;

internal abstract class EntityConfiguration<TEntity>(string tableName)
    : IEntityTypeConfiguration<TEntity> where TEntity : Entity
{

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(u => u.Id);
        builder.ToTable(tableName);
    }
}


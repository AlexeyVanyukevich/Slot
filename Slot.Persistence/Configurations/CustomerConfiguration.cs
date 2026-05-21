using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Slot.Domain.Entities;
using Slot.Domain.Enums;

namespace Slot.Persistence.Configurations;

internal sealed class CustomerConfiguration() : EntityConfiguration<Customer>(TableName)
{
    const string TableName = "customers";

    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Status).HasMaxLength(16).HasConversion(new EnumToStringConverter<CustomerStatus>());

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TenantId, x.Phone }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
    }
}

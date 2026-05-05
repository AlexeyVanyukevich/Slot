using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Slot.Domain.Entities;

namespace Slot.Persistence.Configurations;

internal sealed class ServiceTypeConfiguration() : EntityConfiguration<ServiceType>(TableName)
{
    const string TableName = "service_types";

    public override void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.ServiceTypes)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

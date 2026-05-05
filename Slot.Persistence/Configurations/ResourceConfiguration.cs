using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Slot.Domain.Entities;
using Slot.Domain.Enums;

namespace Slot.Persistence.Configurations;

internal sealed class ResourceConfiguration() : EntityConfiguration<Resource>(TableName)
{
    const string TableName = "resources";

    public override void Configure(EntityTypeBuilder<Resource> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Resources)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

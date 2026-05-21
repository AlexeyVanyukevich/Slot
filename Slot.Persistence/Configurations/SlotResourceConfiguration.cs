using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Slot.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Text;

namespace Slot.Persistence.Configurations;

public class SlotResourceConfiguration : IEntityTypeConfiguration<SlotResource>
{
    const string TableName = "slot_resources";

    public void Configure(EntityTypeBuilder<SlotResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(TableName)
            .HasKey(x => new { x.SlotId, x.ResourceId });

        builder.HasOne(x => x.Resource)
            .WithMany(x => x.Slots)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

namespace Slot.Domain.Entities;

public class SlotResource
{
    public int SlotId { get; set; }
    public SlotEntity Slot { get; set; }
    public int ResourceId { get; set; }
    public Resource Resource { get; set; }
}

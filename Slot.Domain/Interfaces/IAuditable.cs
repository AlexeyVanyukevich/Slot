namespace Slot.Domain.Interfaces;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
}

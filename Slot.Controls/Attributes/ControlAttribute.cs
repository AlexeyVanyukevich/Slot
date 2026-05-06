namespace Slot.Controls.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class ControlAttribute(string field, string label, string? hint = null) : Attribute
{
    public string Field { get; } = field;
    public string Label { get; } = label;
    public string? Hint { get; } = hint;
    public abstract ControlType Type { get; }
}

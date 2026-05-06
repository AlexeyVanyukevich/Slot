namespace Slot.Controls.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class NumberControlAttribute(string field, string label, string? hint = null, int? min = null, int? max = null) : ControlAttribute(field, label, hint)
{
    public override ControlType Type => ControlType.Number;

    public int? Min { get; } = min;
    public int? Max { get; } = max;
}

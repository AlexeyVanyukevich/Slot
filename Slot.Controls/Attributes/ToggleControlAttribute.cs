namespace Slot.Controls.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ToggleControlAttribute(string field, string label, string? hint = null) : ControlAttribute(field, label, hint)
{
    public override ControlType Type => ControlType.Toggle;
}

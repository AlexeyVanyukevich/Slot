namespace Slot.Controls.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class TextControlAttribute(string field, string label, string? hint = null, int? maxLength = null) : ControlAttribute(field, label, hint)
{
    public override ControlType Type => ControlType.Text;

    public int? MaxLength { get; } = maxLength;
}

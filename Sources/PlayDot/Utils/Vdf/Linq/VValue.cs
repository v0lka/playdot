namespace PlayDot.Utils.Vdf.Linq;

public class VValue : VToken
{
    private readonly VTokenType tokenType;

    private VValue(object? value, VTokenType type)
    {
        Value = value;
        tokenType = type;
    }

    public VValue(object? value)
        : this(value, VTokenType.Value)
    {
    }

    public VValue(VValue other)
        : this(other.Value, other.Type)
    {
    }

    public object? Value { get; set; }

    public override VTokenType Type => tokenType;

    public override VToken DeepClone()
    {
        return new VValue(this);
    }

    public override void WriteTo(VdfWriter writer)
    {
        if (tokenType == VTokenType.Comment)
            writer.WriteComment(ToString());
        else
            writer.WriteValue(this);
    }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    public static VValue CreateComment(string value)
    {
        return new VValue(value, VTokenType.Comment);
    }

    public static VValue CreateEmpty()
    {
        return new VValue(string.Empty);
    }

    protected override bool DeepEquals(VToken token)
    {
        if (token is not VValue otherVal) return false;

        return this == otherVal || (Type == otherVal.Type && Value != null && Value.Equals(otherVal.Value));
    }
}
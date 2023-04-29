﻿using System;

namespace PlayDot.Utils.Vdf.Linq;

public class VProperty : VToken
{
    public VProperty(string key, VToken value, VConditional? conditional = null)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        Key = key;
        Value = value;
        Conditional = conditional;
    }

    public VProperty(VProperty other)
        : this(other.Key, other.Value.DeepClone(), (VConditional?) other.Conditional?.DeepClone())
    {
    }

    // Json.NET calls this 'Name', but since VDF is technically KeyValues we call it a 'Key'.
    public string Key { get; set; }
    public VToken Value { get; set; }
    public VConditional? Conditional { get; set; }

    public override VTokenType Type => VTokenType.Property;

    public override VToken DeepClone()
    {
        return new VProperty(this);
    }

    public override void WriteTo(VdfWriter writer)
    {
        writer.WriteKey(Key);
        Value.WriteTo(writer);

        if (Value is VValue && Conditional != null)
            Conditional.WriteTo(writer);
    }

    protected override bool DeepEquals(VToken node)
    {
        return node is VProperty otherProp && Key == otherProp.Key && DeepEquals(Value, otherProp.Value) &&
               DeepEquals(Conditional, otherProp.Conditional);
    }
}
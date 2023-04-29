using System;

namespace PlayDot.Utils.Vdf;

public class VdfException : Exception
{
    public VdfException(string message)
        : base(message)
    {
    }
}
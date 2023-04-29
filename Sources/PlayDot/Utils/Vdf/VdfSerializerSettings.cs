using System.Collections.Generic;

namespace PlayDot.Utils.Vdf;

public class VdfSerializerSettings
{
    /// <summary>
    ///     Sets the size of the token buffer used for deserialization.
    /// </summary>
    public int MaximumTokenSize = 4096;

    /// <summary>
    ///     Determines whether the parser should evaluate conditional blocks ([$WINDOWS], etc.).
    /// </summary>
    public bool UsesConditionals = true;

    /// <summary>
    ///     Determines whether the parser should translate escape sequences (/n, /t, etc.).
    /// </summary>
    public bool UsesEscapeSequences;

    public static VdfSerializerSettings Default => new();

    public static VdfSerializerSettings Common => new()
    {
        UsesEscapeSequences = true,
        UsesConditionals = false
    };

    /// <summary>
    ///     If <see cref="EvaluateConditionals" /> is set to true, only VDF properties 1) without any specified conditional
    ///     logic or 2) conditional logic matching defined conditionals will be returned.
    /// </summary>
    public IReadOnlyList<string>? DefinedConditionals { get; set; }
}
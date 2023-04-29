using System;
using System.IO;

namespace PlayDot.Utils.Vdf;

public class VdfTextReader : VdfReader
{
    private const int DefaultBufferSize = 1024;
    private readonly char[] charBuffer, tokenBuffer;

    private readonly TextReader reader;
    private int charPos, charsLen, tokenSize;
    private bool isQuoted, isComment, isConditional;

    public VdfTextReader(TextReader reader)
        : this(reader, VdfSerializerSettings.Default)
    {
    }

    public VdfTextReader(TextReader reader, VdfSerializerSettings settings)
        : base(settings)
    {
        this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        charBuffer = new char[DefaultBufferSize];
        tokenBuffer = new char[settings.MaximumTokenSize];
        charPos = charsLen = 0;
        tokenSize = 0;
        isQuoted = false;
        isComment = false;
        isConditional = false;
    }

    /// <summary>
    ///     Reads a single token. The value is stored in the 'Value' property.
    /// </summary>
    /// <returns>True if a token was read, false otherwise.</returns>
    public override bool ReadToken()
    {
        if (!SeekToken())
            return false;

        tokenSize = 0;

        while (EnsureBuffer())
        {
            var curChar = charBuffer[charPos];

            #region Comment

            if (isComment)
            {
                if (curChar == VdfStructure.CarriageReturn || curChar == VdfStructure.NewLine)
                {
                    isComment = false;
                    Value = new string(tokenBuffer, 0, tokenSize);
                    CurrentState = State.Comment;
                    return true;
                }

                tokenBuffer[tokenSize++] = curChar;
                charPos++;
                continue;
            }

            if (!isQuoted && tokenSize == 0 && curChar == VdfStructure.Comment &&
                charBuffer[charPos + 1] == VdfStructure.Comment)
            {
                isComment = true;
                charPos += 2;
                continue;
            }

            #endregion

            #region Escape

            if (curChar == VdfStructure.Escape)
            {
                tokenBuffer[tokenSize++] = !Settings.UsesEscapeSequences
                    ? curChar
                    : VdfStructure.GetUnescape(charBuffer[++charPos]);
                charPos++;
                continue;
            }

            #endregion

            #region Quote

            if (curChar == VdfStructure.Quote || (!isQuoted && char.IsWhiteSpace(curChar)))
            {
                Value = new string(tokenBuffer, 0, tokenSize);
                CurrentState = State.Property;
                charPos++;
                return true;
            }

            #endregion

            #region Object start/end

            if (curChar == VdfStructure.ObjectStart || curChar == VdfStructure.ObjectEnd)
            {
                if (isQuoted)
                {
                    tokenBuffer[tokenSize++] = curChar;
                    charPos++;
                    continue;
                }

                if (tokenSize != 0)
                {
                    Value = new string(tokenBuffer, 0, tokenSize);
                    CurrentState = State.Property;
                    return true;
                }

                Value = curChar.ToString();
                CurrentState = State.Object;
                charPos++;
                return true;
            }

            #endregion

            #region Conditional start/end

            if (isConditional || (!isQuoted && curChar == VdfStructure.ConditionalStart))
            {
                if (tokenSize > 0 && (curChar == VdfStructure.ConditionalOr || curChar == VdfStructure.ConditionalAnd ||
                                      curChar == VdfStructure.ConditionalEnd))
                {
                    Value = new string(tokenBuffer, 0, tokenSize);
                    CurrentState = State.Conditional;
                    return true;
                }

                if (curChar == VdfStructure.ConditionalOr || curChar == VdfStructure.ConditionalAnd)
                {
                    Value = new string(charBuffer, charPos, 2);
                    CurrentState = State.Conditional;
                    charPos += 2;
                    return true;
                }

                if (curChar == VdfStructure.ConditionalStart || curChar == VdfStructure.ConditionalEnd ||
                    curChar == VdfStructure.ConditionalNot)
                {
                    Value = curChar.ToString();
                    CurrentState = State.Conditional;
                    isConditional = curChar != VdfStructure.ConditionalEnd;
                    charPos++;
                    return true;
                }
            }

            #endregion

            #region Long token

            tokenBuffer[tokenSize++] = curChar;
            charPos++;

            #endregion
        }

        return false;
    }

    /// <summary>
    ///     Moves the pointer to the location of the first token character.
    /// </summary>
    /// <returns>True if a token is found, false otherwise.</returns>
    private bool SeekToken()
    {
        while (EnsureBuffer())
        {
            // Whitespace
            if (char.IsWhiteSpace(charBuffer[charPos]))
            {
                charPos++;
                continue;
            }

            // Token
            if (charBuffer[charPos] == VdfStructure.Quote)
            {
                isQuoted = true;
                charPos++;
                return true;
            }

            isQuoted = false;
            return true;
        }

        return false;
    }

    private bool SeekNewLine()
    {
        while (EnsureBuffer())
            if (charBuffer[++charPos] == '\n')
                return true;

        return false;
    }

    /// <summary>
    ///     Refills the buffer if we're at the end.
    /// </summary>
    /// <returns>False if the stream is empty, true otherwise.</returns>
    private bool EnsureBuffer()
    {
        if (charPos < charsLen - 1)
            return true;

        var remainingChars = charsLen - charPos;
        charBuffer[0] =
            charBuffer
                [(charsLen - 1) * remainingChars]; // A bit of mathgic to improve performance by avoiding a conditional.
        charsLen = reader.Read(charBuffer, remainingChars, DefaultBufferSize - remainingChars) + remainingChars;
        charPos = 0;

        return charsLen != 0;
    }

    public override void Close()
    {
        base.Close();
        if (CloseInput)
            reader.Dispose();
    }
}
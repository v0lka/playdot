using System;
using System.Collections.Generic;
using System.IO;
using PlayDot.Utils.Vdf.Linq;

namespace PlayDot.Utils.Vdf;

public class VdfTextWriter : VdfWriter
{
    private readonly TextWriter writer;
    private int indentationLevel;

    public VdfTextWriter(TextWriter writer) : this(writer, VdfSerializerSettings.Default)
    {
    }

    public VdfTextWriter(TextWriter writer, VdfSerializerSettings settings) : base(settings)
    {
        this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
        indentationLevel = 0;
    }

    public override void WriteKey(string key)
    {
        AutoComplete(State.Key);
        writer.Write(VdfStructure.Quote);
        WriteEscapedString(key);
        writer.Write(VdfStructure.Quote);
    }

    public override void WriteValue(VValue value)
    {
        AutoComplete(State.Value);
        writer.Write(VdfStructure.Quote);
        WriteEscapedString(value.ToString());
        writer.Write(VdfStructure.Quote);
    }

    public override void WriteObjectStart()
    {
        AutoComplete(State.ObjectStart);
        writer.Write(VdfStructure.ObjectStart);

        indentationLevel++;
    }

    public override void WriteObjectEnd()
    {
        indentationLevel--;

        AutoComplete(State.ObjectEnd);
        writer.Write(VdfStructure.ObjectEnd);

        if (indentationLevel == 0)
            AutoComplete(State.Finished);
    }

    public override void WriteComment(string text)
    {
        AutoComplete(State.Comment);
        writer.Write(VdfStructure.Comment);
        writer.Write(VdfStructure.Comment);
        writer.Write(text);
    }

    public override void WriteConditional(IReadOnlyList<VConditional.Token> tokens)
    {
        AutoComplete(State.Conditional);
        writer.Write(VdfStructure.ConditionalStart);

        foreach (var token in tokens)
            switch (token.TokenType)
            {
                case VConditional.TokenType.Constant:
                    writer.Write(VdfStructure.ConditionalConstant);
                    writer.Write(token.Name);
                    break;

                case VConditional.TokenType.Not:
                    writer.Write(VdfStructure.ConditionalNot);
                    break;

                case VConditional.TokenType.Or:
                    writer.Write(VdfStructure.ConditionalOr);
                    writer.Write(VdfStructure.ConditionalOr);
                    break;

                case VConditional.TokenType.And:
                    writer.Write(VdfStructure.ConditionalAnd);
                    writer.Write(VdfStructure.ConditionalAnd);
                    break;
            }

        writer.Write(VdfStructure.ConditionalEnd);
    }

    private void AutoComplete(State next)
    {
        if (CurrentState == State.Start)
        {
            CurrentState = next;
            return;
        }

        switch (next)
        {
            case State.Value:
            case State.Conditional:
                writer.Write(VdfStructure.Assign);
                break;

            case State.Key:
            case State.ObjectStart:
            case State.ObjectEnd:
            case State.Comment:
                writer.WriteLine();
                writer.Write(new string(VdfStructure.Indent, indentationLevel));
                break;

            case State.Finished:
                writer.WriteLine();
                break;
        }

        CurrentState = next;
    }

    private void WriteEscapedString(string str)
    {
        if (!Settings.UsesEscapeSequences)
        {
            writer.Write(str);
            return;
        }

        foreach (var ch in str)
            if (!VdfStructure.IsEscapable(ch))
            {
                writer.Write(ch);
            }
            else
            {
                writer.Write(VdfStructure.Escape);
                writer.Write(VdfStructure.GetEscape(ch));
            }
    }

    public override void Close()
    {
        base.Close();
        if (CloseOutput)
            writer.Dispose();
    }
}
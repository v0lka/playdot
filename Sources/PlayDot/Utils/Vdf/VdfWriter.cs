using System;
using System.Collections.Generic;
using PlayDot.Utils.Vdf.Linq;

namespace PlayDot.Utils.Vdf;

public abstract class VdfWriter : IDisposable
{
    protected VdfWriter() : this(VdfSerializerSettings.Default)
    {
    }

    protected VdfWriter(VdfSerializerSettings settings)
    {
        Settings = settings;

        CurrentState = State.Start;
        CloseOutput = true;
    }

    public VdfSerializerSettings Settings { get; }
    public bool CloseOutput { get; set; }
    protected internal State CurrentState { get; protected set; }

    void IDisposable.Dispose()
    {
        if (CurrentState == State.Closed)
            return;

        Close();
    }

    public abstract void WriteObjectStart();

    public abstract void WriteObjectEnd();

    public abstract void WriteKey(string key);

    public abstract void WriteValue(VValue value);

    public abstract void WriteComment(string text);

    public abstract void WriteConditional(IReadOnlyList<VConditional.Token> tokens);

    public virtual void Close()
    {
        CurrentState = State.Closed;
    }

    protected internal enum State
    {
        Start,
        Key,
        Value,
        ObjectStart,
        ObjectEnd,
        Comment,
        Conditional,
        Finished,
        Closed
    }
}
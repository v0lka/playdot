using System;

namespace PlayDot.Utils.Vdf;

public abstract class VdfReader : IDisposable
{
    protected VdfReader()
        : this(VdfSerializerSettings.Default)
    {
    }

    protected VdfReader(VdfSerializerSettings settings)
    {
        Settings = settings;

        CurrentState = State.Start;
        Value = null;
        CloseInput = true;
    }

    public VdfSerializerSettings Settings { get; }
    public bool CloseInput { get; set; }
    public string Value { get; set; }

    protected internal State CurrentState { get; protected set; }

    void IDisposable.Dispose()
    {
        if (CurrentState == State.Closed)
            return;

        Close();
    }

    public abstract bool ReadToken();

    public virtual void Close()
    {
        CurrentState = State.Closed;
        Value = null;
    }

    protected internal enum State
    {
        Start,
        Property,
        Object,
        Comment,
        Conditional,
        Finished,
        Closed
    }
}
namespace Engage.FC;

public abstract class StackAction
{
    public string Type { get; init; }

    protected StackAction(string type)
    {
        Type = type;
    }
}

public class StackPop : StackAction
{
    public override string ToString()
        => $"-{Type}";

    public StackPop(string type) : base(type)
    {
    }
}

public class StackPopS : StackAction
{
    public override string ToString()
        => $"-{Type}*";

    public StackPopS(string type) : base(type)
    {
    }
}

public class StackPush : StackAction
{
    public override string ToString()
        => $"+{Type}";

    public StackPush(string type) : base(type)
    {
    }
}

public class StackAwait : StackAction
{
    public override string ToString()
        => $"?{Type}";

    public StackAwait(string type) : base(type)
    {
    }
}

public class StackAwaitAll : StackAction
{
    public override string ToString()
        => $"?{Type}*";

    public StackAwaitAll(string type) : base(type)
    {
    }
}
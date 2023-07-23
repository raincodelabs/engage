using System.Collections.Generic;

namespace Engage.FC;

public abstract class StackAction
{
    public string Type { get; init; }

    protected StackAction(string type)
    {
        Type = type;
    }

    public abstract bool Apply(Stack<FC.StackAction> stack);
}

public class StackPop : StackAction
{
    public override string ToString()
        => $"-{Type}";

    public StackPop(string type) : base(type)
    {
    }

    public override bool Apply(Stack<FC.StackAction> stack)
    {
        if (stack.Count > 0 && stack.Peek() is FC.StackPush push && push.Type == Type)
        {
            stack.Pop();
            return true;
        }

        return false;
    }
}

public class StackPopS : StackAction
{
    public override string ToString()
        => $"-{Type}*";

    public StackPopS(string type) : base(type)
    {
    }

    public override bool Apply(Stack<StackAction> stack)
    {
        if (stack.Count > 0 && stack.Peek() is FC.StackPush top && top.Type == Type)
        {
            while (stack.Peek() is FC.StackPush push && push.Type == Type)
                stack.Pop();
            return true;
        }

        return false;
    }
}

public class StackPush : StackAction
{
    public override string ToString()
        => $"+{Type}";

    public StackPush(string type) : base(type)
    {
    }

    public override bool Apply(Stack<StackAction> stack)
    {
        if (stack.Count > 0)
        {
            if (stack.Peek() is FC.StackAwait await1 && await1.Type == Type)
            {
                stack.Pop();
                return true;
            }
            else if (stack.Peek() is FC.StackAwaitAll awaitMany && awaitMany.Type == Type)
            {
                return true;
            }
            else
                stack.Push(this);
        }
        else
            stack.Push(this);

        return true;
    }
}

public class StackAwait : StackAction
{
    public override string ToString()
        => $"?{Type}";

    public StackAwait(string type) : base(type)
    {
    }

    public override bool Apply(Stack<StackAction> stack)
    {
        stack.Push(this);
        return true;
    }
}

public class StackAwaitAll : StackAction
{
    public override string ToString()
        => $"?{Type}*";

    public StackAwaitAll(string type) : base(type)
    {
    }

    public override bool Apply(Stack<StackAction> stack)
    {
        stack.Push(this);
        return true;
    }
}
using System.Collections.Generic;

namespace Engage.NC;

public abstract class Reaction
{
    public string Name = "";

    public virtual NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => null;

    internal virtual IEnumerable<string> GetFlags()
        => new List<string>();

    internal abstract IEnumerable<FC.SignedFlag> ToFlagActions();
    internal abstract IEnumerable<FC.StackAction> ToStackActions();
}
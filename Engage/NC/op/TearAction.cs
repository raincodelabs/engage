using System.Collections.Generic;
using Engage.FC;

namespace Engage.NC;

public class TearAction : Reaction
{
    public override bool Equals(object obj)
    {
        var other = obj as TearAction;
        if (other == null)
            return false;
        return Name == other.Name;
    }

    //NB: using various fields with various names is future work
    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.TearOne { Name = NA.SystemPlan.Dealias(Name), Target = "value" };

    internal override IEnumerable<FC.SignedFlag> ToFlagActions()
        => new List<FC.SignedFlag>();

    internal override IEnumerable<FC.StackAction> ToStackActions()
    {
        // TODO: test
        var inferredType = NA.SystemPlan.Dealias(Name);
        return new List<FC.StackAction> { new StackPop(inferredType) };
    }
}
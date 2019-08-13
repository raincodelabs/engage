using System.Collections.Generic;

namespace Engage.A
{
    public class Reaction
    {
        public virtual B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => null;
    }

    public partial class WrapReaction : Reaction
    {
        public string Name;
        public List<string> Args = new List<string>();

        // NB: in this case the "target" argument is actually the type since we know the [intermediate] target from the call
        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.WrapOne(Name, target, Args[0]);
    }

    public partial class PushReaction : Reaction
    {
        public string Name;
        public List<string> Args = new List<string>();

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PushNew(Name, Args);
    }

    public partial class LiftReaction : Reaction
    {
        public string Flag;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.LiftFlag() { Flag = Flag };
    }

    public partial class DropReaction : Reaction
    {
        public string Flag;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.DropFlag() { Flag = Flag };
    }

    public partial class TrimReaction : Reaction
    {
        public string Type;
        public bool Starred;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.TrimStream() { Type = Type, Starred = Starred };
    }
}
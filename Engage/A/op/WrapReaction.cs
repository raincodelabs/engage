using System.Collections.Generic;

namespace Engage.A
{
    public partial class WrapReaction : Reaction
    {
        public List<string> Args = new List<string>();

        // NB: in this case the "target" argument is actually the type since we know the [intermediate] target from the call
        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.WrapOne(Name, target, Args[0]);
    }
}
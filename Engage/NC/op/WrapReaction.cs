using System.Collections.Generic;
using System.Linq;

namespace Engage.NC
{
    public class WrapReaction : Reaction
    {
        public List<string> Args = new List<string>();

        public override bool Equals(object obj)
        {
            var other = obj as WrapReaction;
            if (other == null)
                return false;
            return Name == other.Name
                   && Args.SequenceEqual(other.Args);
        }

        // NB: in this case the "target" argument is actually the type since we know the [intermediate] target from the call
        public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
            => new NA.WrapOne(Name, target, Args[0]);
    }
}
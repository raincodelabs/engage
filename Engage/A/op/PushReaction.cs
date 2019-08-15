using System.Collections.Generic;

namespace Engage.A
{
    public partial class PushReaction : Reaction
    {
        public List<string> Args = new List<string>();

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var t = prev as B.TearOne;
            if (t == null)
                return new B.PushNew(Name, Args, "");
            else
                return new B.PushNew(Name, Args, tearing: $"{t.Name}.{t.Target}");
        }
    }
}
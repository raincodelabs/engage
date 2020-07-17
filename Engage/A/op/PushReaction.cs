using System.Collections.Generic;

namespace Engage.A
{
    public class PushReaction : Reaction
    {
        public List<string> Args = new List<string>();

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var t = prev as B.TearOne;
            return t == null
                ? new B.PushNew(Name, Args, "")
                : new B.PushNew(Name, Args, $"{t.Name}.{t.Target}");
        }
    }
}
using System.Collections.Generic;

namespace Engage.A
{
    public partial class PushReaction : Reaction
    {
        public string Name;
        public List<string> Args = new List<string>();

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PushNew(Name, Args);
    }
}
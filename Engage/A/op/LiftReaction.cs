using System.Collections.Generic;

namespace Engage.A
{
    public partial class LiftReaction : Reaction
    {
        public string Flag;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.LiftFlag() { Flag = Flag };

        internal override IEnumerable<string> GetFlags()
            => new List<string>() { Flag };
    }
}
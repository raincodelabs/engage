using System.Collections.Generic;

namespace Engage.A
{
    public partial class DropReaction : Reaction
    {
		public string Flag;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.DropFlag() { Flag = Flag };

        internal override IEnumerable<string> GetFlags()
            => new List<string>() { Flag };
    }
}
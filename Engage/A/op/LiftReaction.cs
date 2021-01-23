using System.Collections.Generic;

namespace Engage.A
{
    public class LiftReaction : Reaction
    {
        public string Flag;

        public override bool Equals(object obj)
        {
            var other = obj as LiftReaction;
            if (other == null)
                return false;
            return Name == other.Name
                   && Flag == other.Flag;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.LiftFlag { Flag = Flag };

        internal override IEnumerable<string> GetFlags()
            => new List<string> { Flag };
    }
}
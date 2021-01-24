using System;
using System.Collections.Generic;

namespace Engage.A
{
    public class DropReaction : Reaction
    {
        public string Flag = "";

        public override bool Equals(object obj)
        {
            var other = obj as DropReaction;
            if (other == null)
            {
                Console.WriteLine("[x] DropReaction compared to non-DropReaction");
                return false;
            }

            if (Name != other.Name)
            {
                Console.WriteLine("[x] DropReaction: Name mismatch");
                return false;
            }

            if (Flag != other.Flag)
            {
                Console.WriteLine("[x] DropReaction: Flag mismatch");
                return false;
            }

            Console.WriteLine("[√] DropReaction == DropReaction");
            return true;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.DropFlag { Flag = Flag };

        internal override IEnumerable<string> GetFlags()
            => new List<string> { Flag };
    }
}
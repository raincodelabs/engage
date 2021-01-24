using System;
using System.Collections.Generic;

namespace Engage.A
{
    public class LiftReaction : Reaction
    {
        public string Flag = "";

        public override bool Equals(object obj)
        {
            var other = obj as LiftReaction;
            if (other == null)
            {
                Console.WriteLine("[x] LiftReaction compared to non-LiftReaction");
                return false;
            }

            if (Name != other.Name)
            {
                Console.WriteLine($"[x] LiftReaction: Name mismatch ({Name} vs {other.Name})");
                return false;
            }

            if (Flag != other.Flag)
            {
                Console.WriteLine("[x] LiftReaction: Flag mismatch");
                return false;
            }

            Console.WriteLine("[√] LiftReaction == LiftReaction");
            return true;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.LiftFlag { Flag = Flag };

        internal override IEnumerable<string> GetFlags()
            => new List<string> { Flag };
    }
}
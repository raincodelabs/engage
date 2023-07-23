using System;

namespace Engage.NC
{
    public class Assignment
    {
        public string LHS;
        public Reaction RHS;

        public override bool Equals(object obj)
        {
            var other = obj as Assignment;
            if (other == null)
            {
                Console.WriteLine("[x] Assignment compared to non-Assignment");
                return false;
            }

            if (LHS != other.LHS)
            {
                Console.WriteLine("[x] Assignment: LHS mismatch");
                return false;
            }

            if (!RHS.Equals(other.RHS))
            {
                Console.WriteLine("[x] Assignment: RHS mismatch");
                return false;
            }

            Console.WriteLine("[√] Assignment == Assignment");
            return true;
        }
    }
}
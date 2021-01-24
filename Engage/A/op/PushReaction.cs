using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.A
{
    public class PushReaction : Reaction
    {
        public List<string> Args = new List<string>();

        public override bool Equals(object obj)
        {
            var other = obj as PushReaction;
            if (other == null)
            {
                Console.WriteLine("[x] PushReaction compared to non-PushReaction");
                return false;
            }

            if (Name != other.Name)
            {
                Console.WriteLine("[x] PushReaction: Name mismatch");
                return false;
            }

            if (!Args.SequenceEqual(other.Args))
            {
                Console.WriteLine("[x] PushReaction: Args mismatch");
                return false;
            }

            Console.WriteLine("[√] PushReaction == PushReaction");
            return true;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var t = prev as B.TearOne;
            return t == null
                ? new B.PushNew(Name, Args, "")
                : new B.PushNew(Name, Args, $"{t.Name}.{t.Target}");
        }
    }
}
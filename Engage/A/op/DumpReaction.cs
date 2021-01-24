using System;

namespace Engage.A
{
    public class DumpReaction : Reaction
    {
        public DumpReaction(string type)
        {
            Name = type ?? String.Empty;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DumpReaction;
            if (other == null)
                return false;
            return Name == other.Name;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.DumpOne(Name);
    }
}
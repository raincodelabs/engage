namespace Engage.A
{
    public class TrimReaction : Reaction
    {
        public bool Starred;

        public override bool Equals(object obj)
        {
            var other = obj as TrimReaction;
            if (other == null)
                return false;
            return Name == other.Name
                   && Starred == other.Starred;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.TrimStream { Type = Name, Starred = Starred };
    }
}
namespace Engage.A
{
    public partial class TrimReaction : Reaction
    {
        public bool Starred;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.TrimStream() { Type = Name, Starred = Starred };
    }
}
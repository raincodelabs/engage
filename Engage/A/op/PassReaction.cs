namespace Engage.A
{
    public class PassReaction : Reaction
    {
        public override bool Equals(object obj)
            => obj is PassReaction;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.DoNothing();
    }
}
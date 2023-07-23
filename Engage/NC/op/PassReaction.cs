namespace Engage.NC
{
    public class PassReaction : Reaction
    {
        public override bool Equals(object obj)
            => obj is PassReaction;

        public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
            => new NA.DoNothing();
    }
}
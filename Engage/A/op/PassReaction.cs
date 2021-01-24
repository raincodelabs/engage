namespace Engage.A
{
    public class PassReaction : Reaction
    {
        public override bool Equals(object obj)
        {
            var other = obj as PassReaction;
            return other != null;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.DoNothing();
    }
}
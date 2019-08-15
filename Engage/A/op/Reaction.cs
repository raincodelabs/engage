namespace Engage.A
{
    public class Reaction
    {
        public string Name;

        public virtual B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => null;
    }
}
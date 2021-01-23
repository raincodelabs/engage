namespace Engage.A
{
    public class PopAction : Reaction
    {
        public override bool Equals(object obj)
        {
            var other = obj as PopAction;
            if (other == null)
                return false;
            return Name == other.Name;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PopOne { Name = B.SystemPlan.Dealias(Name), Target = target };
    }
}
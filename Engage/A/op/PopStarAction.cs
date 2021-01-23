namespace Engage.A
{
    public class PopStarAction : Reaction
    {
        public override bool Equals(object obj)
        {
            var other = obj as PopStarAction;
            if (other == null)
                return false;
            return Name == other.Name;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PopAll {Name = B.SystemPlan.Dealias(Name), Target = target};
    }
}
namespace Engage.NC
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

        public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
            => new NA.PopAll { Name = NA.SystemPlan.Dealias(Name), Target = target };
    }
}
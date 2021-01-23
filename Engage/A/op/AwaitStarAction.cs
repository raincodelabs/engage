namespace Engage.A
{
    public class AwaitStarAction : Reaction
    {
        public string TmpContext;

        public override bool Equals(object obj)
        {
            var other = obj as AwaitStarAction;
            if (other == null)
                return false;
            return Name == other.Name
                   && TmpContext == other.TmpContext;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.AwaitMany
            {
                Name = B.SystemPlan.Dealias(Name),
                Target = target,
                Flag = TmpContext,
                BaseAction = prev
            };
            return a;
        }
    }
}
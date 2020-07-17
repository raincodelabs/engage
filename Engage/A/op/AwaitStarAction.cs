namespace Engage.A
{
    public class AwaitStarAction : Reaction
    {
        public string TmpContext;

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
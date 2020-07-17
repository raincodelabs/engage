namespace Engage.A
{
    public class PopStarAction : Reaction
    {
        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PopAll { Name = B.SystemPlan.Dealias(Name), Target = target };
    }
}
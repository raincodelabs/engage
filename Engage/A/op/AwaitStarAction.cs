namespace Engage.A
{
    public partial class AwaitStarAction : Reaction
    {
        public string TmpContext;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.AwaitMany() { Name = B.SystemPlan.Dealias(Name), Target = target, Flag = TmpContext };
            a.BaseAction = prev;
            return a;
        }
    }
}
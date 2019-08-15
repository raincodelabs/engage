namespace Engage.A
{
    public partial class PopHashAction : Reaction
    {
        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.PopSeveral() { Name = B.SystemPlan.Dealias(Name), Target = target };
            a.SiblingActions.Add(prev);
            if (prev is B.PopSeveral ps)
                a.SiblingActions.AddRange(ps.SiblingActions);
            return a;
        }
    }
}
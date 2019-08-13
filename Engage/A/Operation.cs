namespace Engage.A
{
    public partial class PopAction : Reaction
    {
        public string Name;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PopOne() { Name = B.SystemPlan.Dealias(Name), Target = target };
    }

    public partial class PopStarAction : Reaction
    {
        public string Name;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PopAll() { Name = B.SystemPlan.Dealias(Name), Target = target };
    }

    public partial class PopHashAction : Reaction
    {
        public string Name;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.PopSeveral() { Name = B.SystemPlan.Dealias(Name), Target = target };
            a.SiblingActions.Add(prev);
            if (prev is B.PopSeveral ps)
                a.SiblingActions.AddRange(ps.SiblingActions);
            return a;
        }
    }

    public partial class AwaitAction : Reaction
    {
        public string Name;
        public string TmpContext;
        public string ExtraContext;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.AwaitOne() { Name = B.SystemPlan.Dealias(Name), Target = target, Flag = TmpContext, ExtraFlag = ExtraContext };
            a.BaseAction = prev;
            return a;
        }
    }

    public partial class AwaitStarAction : Reaction
    {
        public string Name;
        public string TmpContext;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.AwaitMany() { Name = B.SystemPlan.Dealias(Name), Target = target, Flag = TmpContext };
            a.BaseAction = prev;
            return a;
        }
    }
}
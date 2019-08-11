namespace Engage.A
{
    public partial class Operation
    {
        internal virtual B.HandleAction ToHandleAction(string target, B.HandleAction prev = null)
            => null;
    }

    public partial class PopAction : Operation
    {
        public string Name;

        internal override B.HandleAction ToHandleAction(string target, B.HandleAction prev = null)
            => new B.PopOne() { Name = Name, Target = target };
    }

    public partial class PopStarAction : Operation
    {
        public string Name;

        internal override B.HandleAction ToHandleAction(string target, B.HandleAction prev = null)
            => new B.PopAll() { Name = Name, Target = target };
    }

    public partial class PopHashAction : Operation
    {
        public string Name;

        internal override B.HandleAction ToHandleAction(string target, B.HandleAction prev = null)
        {
            var a = new B.PopSeveral() { Name = Name, Target = target };
            a.SiblingActions.Add(prev);
            if (prev is B.PopSeveral ps)
                a.SiblingActions.AddRange(ps.SiblingActions);
            return a;
        }
    }

    public partial class AwaitAction : Operation
    {
        public string Name;
        public string TmpContext;
        public string ExtraContext;

        internal override B.HandleAction ToHandleAction(string target, B.HandleAction prev = null)
        {
            var a = new B.AwaitOne() { Name = Name, Target = target, Flag = TmpContext, ExtraFlag = ExtraContext };
            a.BaseAction = prev;
            return a;
        }
    }

    public partial class AwaitStarAction : Operation
    {
        public string Name;
        public string TmpContext;
    }
}
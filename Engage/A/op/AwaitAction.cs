using System.Collections.Generic;

namespace Engage.A
{
    public class AwaitAction : Reaction
    {
        public string TmpContext;
        public string ExtraContext;

        public override bool Equals(object obj)
        {
            var other = obj as AwaitAction;
            if (other == null)
                return false;
            return Name == other.Name
                   && TmpContext == other.TmpContext
                   && ExtraContext == other.ExtraContext
                ;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.AwaitOne
            {
                Name = B.SystemPlan.Dealias(Name),
                Target = target,
                Flag = TmpContext,
                ExtraFlag = ExtraContext,
                BaseAction = prev
            };
            return a;
        }

        internal override IEnumerable<string> GetFlags()
            => new List<string>() { TmpContext, ExtraContext };
    }
}
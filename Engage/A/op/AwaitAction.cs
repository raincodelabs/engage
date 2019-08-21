using System.Collections.Generic;

namespace Engage.A
{
    public partial class AwaitAction : Reaction
    {
        public string TmpContext;
        public string ExtraContext;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.AwaitOne() { Name = B.SystemPlan.Dealias(Name), Target = target, Flag = TmpContext, ExtraFlag = ExtraContext };
            a.BaseAction = prev;
            return a;
        }

        internal override IEnumerable<string> GetFlags()
            => new List<string>() { TmpContext, ExtraContext };
    }
}
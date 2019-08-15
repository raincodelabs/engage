using Engage.B;

namespace Engage.A
{
    public class TearAction : Reaction
    {
        //NB: using various fields with various names is future work
        public override HandleAction ToHandleAction(string target = "", HandleAction prev = null)
            => new B.TearOne() { Name = B.SystemPlan.Dealias(Name), Target = "value" };
    }
}
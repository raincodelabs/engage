using Engage.B;

namespace Engage.A
{
    public class TearAction : Reaction
    {
        public override bool Equals(object obj)
        {
            var other = obj as TearAction;
            if (other == null)
                return false;
            return Name == other.Name;
        }

        //NB: using various fields with various names is future work
        public override HandleAction ToHandleAction(string target = "", HandleAction prev = null)
            => new B.TearOne() {Name = B.SystemPlan.Dealias(Name), Target = "value"};
    }
}
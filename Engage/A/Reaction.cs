using System.Collections.Generic;

namespace Engage.A
{
    public class Reaction
    {
        public virtual B.HandleAction ToHandleAction()
            => null;
    }

    public partial class PushReaction : Reaction
    {
        public string Name;
        public List<string> Args = new List<string>();

        public override B.HandleAction ToHandleAction()
            => new B.PushNew(Name, Args);
    }

    public partial class LiftReaction : Reaction
    {
        public string Flag;

        public override B.HandleAction ToHandleAction()
            => new B.LiftFlag() { Flag = Flag };
    }

    public partial class DropReaction : Reaction
    {
        public string Flag;

        public override B.HandleAction ToHandleAction()
            => new B.DropFlag() { Flag = Flag };
    }
}
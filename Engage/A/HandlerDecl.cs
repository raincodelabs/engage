using System.Collections.Generic;

namespace Engage.A
{
    public partial class HandlerDecl
    {
        public Trigger LHS;
        public Reaction RHS;
        public List<Assignment> Context = new List<Assignment>();

        internal Reaction GetContext(string name)
        {
            foreach (var a in Context)
                if (a.LHS == name)
                    return a.RHS;
            return null;
        }
    }
}
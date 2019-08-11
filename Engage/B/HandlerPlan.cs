using System.Collections.Generic;

namespace Engage.B
{
    public class HandlerPlan
    {
        public TokenPlan ReactOn;
        public string GuardFlag;
        public List<HandleAction> Recipe = new List<HandleAction>();
    }
}
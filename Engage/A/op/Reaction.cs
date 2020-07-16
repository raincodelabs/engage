using System.Collections.Generic;

namespace Engage.A
{
    public class Reaction
    {
		public string Name;

        public virtual B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => null;

        internal virtual IEnumerable<string> GetFlags()
            => new List<string>();
    }
}
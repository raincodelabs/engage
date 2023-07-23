using System.Collections.Generic;

namespace Engage.GC
{
    public abstract class CsTop
    {
        public string Name;

        public abstract void GenerateCode(List<string> lines, int level);
    }
}
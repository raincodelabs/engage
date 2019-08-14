using System.Collections.Generic;

namespace Engage.D
{
    public abstract class CsTop
    {
        public string Name;

        public abstract void GenerateCode(List<string> lines, int level);
    }
}
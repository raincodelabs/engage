using System.Collections.Generic;

namespace Engage.C
{
    /// <summary>
    /// Almost code / abstract code
    /// </summary>

    public abstract class CsTop
    {
        public string Name;

        public abstract void GenerateCode(List<string> lines, int level);
    }
}
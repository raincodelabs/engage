using System.Collections.Generic;

namespace Engage.D
{
    public abstract class CsStmt
    {
        public abstract void GenerateCode(List<string> lines, int level);
    }
}
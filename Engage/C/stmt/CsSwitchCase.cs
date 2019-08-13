using System.Collections.Generic;

namespace Engage.C
{
    public class CsSwitchCase : CsStmt
    {
        public string Expression;
        public Dictionary<string, List<CsStmt>> Branches = new Dictionary<string, List<CsStmt>>();
        public List<CsStmt> DefaultBranch = new List<CsStmt>();

        public override void GenerateCode(List<string> lines, int level)
        {
            lines.Add(level, $"switch ({Expression})");
            lines.Open(level);
            foreach (var cond in Branches.Keys)
            {
                lines.Add(level + 1, $"case {cond}:");
                foreach (var line in Branches[cond])
                    line.GenerateCode(lines, level + 2);
                lines.Add(level + 2, "break;");
                lines.Empty();
            }
            if (DefaultBranch.Count > 0)
            {
                lines.Add(level + 1, "default:");
                foreach (var line in DefaultBranch)
                    line.GenerateCode(lines, level + 2);
                lines.Add(level + 2, "break;");
                lines.Empty();
            }
            lines.Close(level);
        }
    }
}
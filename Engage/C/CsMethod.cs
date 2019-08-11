using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class CsMethod : CsExeField
    {
        public string Name;
        public string RetType;

        public override void GenerateCode(List<string> lines, int level, string className)
        {
            string args = String.Join(", ", Args.Select(a => $"{a.Item2} _{a.Item1}"));
            lines.Add(level, $"{(IsPublic ? "public" : "private")} {RetType} {Name}({args})");
            lines.Open(level);
            foreach (var line in Code)
                line.GenerateCode(lines, level + 1);
            lines.Close(level);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class CsConstructor : CsExeField
    {
        public override void GenerateCode(List<string> lines, int level, string className)
        {
            string args = String.Join(", ", Args.Select(a => $"{a.Item2} _{a.Item1}"));
            lines.Add(level, $"{(IsPublic ? "public" : "private")} {className}({args})");
            lines.Open(level);
            foreach (var a in Args)
                if (a.Item2.IsCollection())
                    lines.Add(level + 1, $"{a.Item1}.AddRange(_{a.Item1});");
                else
                    lines.Add(level + 1, $"{a.Item1} = _{a.Item1};");
            foreach (var line in Code)
                line.GenerateCode(lines, level + 1);
            lines.Close(level);
        }
    }
}
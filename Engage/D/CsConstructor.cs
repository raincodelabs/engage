using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.D
{
    public class CsConstructor : CsExeField
    {
        public bool InheritFromBase = false;

        public CsConstructor(bool inheritFromBase, bool isPublic, IEnumerable<Tuple<string, string>> args, IEnumerable<CsStmt> code)
        {
            InheritFromBase = inheritFromBase;
            IsPublic = isPublic;
            Args.AddRange(args);
            Code.AddRange(code);
        }

        public override void GenerateCode(List<string> lines, int level, string className)
        {
            string args1 = String.Join(", ", Args.Select(a => $"{a.Item2} _{a.Item1}"));
            string args2 = String.Join(", ", Args.Select(a => $"_{a.Item1}"));
            lines.Add(level, $"{(IsPublic ? "public" : "private")} {className}({args1}){(InheritFromBase ? $" : base({args2})" : "")}");
            lines.Open(level);
            if (!InheritFromBase)
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
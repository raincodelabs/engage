using System;
using System.Collections.Generic;

namespace Engage.C
{
    public abstract class CsStmt
    {
        public abstract void GenerateCode(List<string> lines, int level);
    }

    public class CsSimpleStmt : CsStmt
    {
        public string Code;

        public CsSimpleStmt()
        {
        }

        public CsSimpleStmt(string code)
        {
            Code = code;
        }

        public override void GenerateCode(List<string> lines, int level)
        {
            if (!Code.EndsWith(";"))
                Code += ";";
            lines.Add(level, Code);
        }
    }

    public class CsComplexStmt : CsStmt
    {
        public string Before, After;
        public List<CsStmt> Code = new List<CsStmt>();

        public CsComplexStmt()
        {
        }

        public CsComplexStmt(string before, CsStmt code, string after = "")
        {
            Before = before;
            Code.Add(code);
            After = after;
        }

        public CsComplexStmt(string before, string code, string after = "")
            : this(before, new CsSimpleStmt(code), after)
        {
        }

        public void AddCode(string stmt)
            => AddCode(new CsSimpleStmt(stmt));

        public void AddCode(string cond, string line)
            => AddCode(new CsComplexStmt(cond, line));

        public void AddCode(CsStmt stmt)
            => Code.Add(stmt);

        public override void GenerateCode(List<string> lines, int level)
        {
            if (!String.IsNullOrEmpty(Before))
                lines.Add(level, Before);
            lines.Open(level);
            foreach (var stmt in Code)
                stmt.GenerateCode(lines, level + 1);
            lines.Close(level);
            if (!String.IsNullOrEmpty(After))
            {
                if (!After.EndsWith(";"))
                    After += ";";
                lines.Add(level, After);
            }
        }
    }

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
using System;
using System.Collections.Generic;

namespace Engage.D
{
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

        public CsComplexStmt(string before, IEnumerable<CsStmt> code, string after)
        {
            Before = before;
            Code.AddRange(code);
            After = after;
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
            if (Code.Count > 1)
                lines.Open(level);
            foreach (var stmt in Code)
                stmt.GenerateCode(lines, level + 1);
            if (Code.Count > 1)
                lines.Close(level);
            if (!String.IsNullOrEmpty(After))
            {
                if (!After.EndsWith(";"))
                    After += ";";
                lines.Add(level, After);
            }
        }
    }
}
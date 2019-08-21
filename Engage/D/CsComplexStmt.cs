using System;
using System.Collections.Generic;

namespace Engage.D
{
    public class CsComplexStmt : CsStmt
    {
        public string Before, After;
        private List<CsStmt> Code = new List<CsStmt>();
        public bool Embrace = true;

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

        public void AddCode(IEnumerable<CsStmt> stmts)
            => Code.AddRange(stmts);

        public override void GenerateCode(List<string> lines, int level)
        {
            if (!String.IsNullOrEmpty(Before))
                lines.Add(level, Before);
            if (Embrace && Code.Count > 1)
                lines.Open(level);
            foreach (var stmt in Code)
                stmt.GenerateCode(lines, level + 1);
            if (Embrace && Code.Count > 1)
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
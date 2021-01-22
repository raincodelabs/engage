using System;
using System.Collections.Generic;

namespace Engage.D
{
    public class CsComplexStmt : CsStmt
    {
        public string Before, After;
        private readonly List<CsStmt> _code = new List<CsStmt>();
        public bool Embrace = true;

        public CsComplexStmt()
        {
        }

        public CsComplexStmt(string before, CsStmt code, string after = "")
        {
            Before = before;
            _code.Add(code);
            After = after;
        }

        public CsComplexStmt(string before, string code, string after = "")
            : this(before, new CsSimpleStmt(code), after)
        {
        }

        public CsComplexStmt(string before, IEnumerable<CsStmt> code, string after)
        {
            Before = before;
            _code.AddRange(code);
            After = after;
        }

        public void AddCode(string stmt)
            => AddCode(new CsSimpleStmt(stmt));

        public void AddCode(string cond, string line)
            => AddCode(new CsComplexStmt(cond, line));

        public void AddCode(CsStmt stmt)
            => _code.Add(stmt);

        public void AddCode(IEnumerable<CsStmt> stmts)
            => _code.AddRange(stmts);

        public override void GenerateCode(List<string> lines, int level)
        {
            if (!String.IsNullOrEmpty(Before))
                lines.Add(level, Before);
            if (Embrace && (_code.Count > 1 || Before.Contains("switch")))
                lines.Open(level);
            foreach (var stmt in _code)
                stmt.GenerateCode(lines, level + 1);
            if (Embrace && (_code.Count > 1 || Before.Contains("switch")))
                lines.Close(level);
            if (String.IsNullOrEmpty(After)) return;
            if (!After.EndsWith(";"))
                After += ";";
            lines.Add(level, After);
        }
    }
}
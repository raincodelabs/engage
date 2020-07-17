using System;
using System.Collections.Generic;
using System.Text;

namespace Engage.D
{
    public class CsStmtList : CsStmt
    {
        private readonly List<CsStmt> Stmts = new List<CsStmt>();

        public CsStmtList()
        {
        }

        public CsStmtList(IEnumerable<CsStmt> stmts)
        {
            Stmts.AddRange(stmts);
        }

        public void AddStmt(CsStmt stmt)
            => Stmts.Add(stmt);

        public override void GenerateCode(List<string> lines, int level)
        {
            foreach (var stmt in Stmts)
                stmt.GenerateCode(lines, level);
        }
    }
}

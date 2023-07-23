using System.Collections.Generic;

namespace Engage.D
{
    public class CsStmtList : CsStmt
    {
        private readonly List<CsStmt> _stmts = new();

        public CsStmtList()
        {
        }

        public CsStmtList(IEnumerable<CsStmt> stmts)
        {
            _stmts.AddRange(stmts);
        }

        public void AddStmt(CsStmt stmt)
            => _stmts.Add(stmt);

        public override void GenerateCode(List<string> lines, int level)
        {
            foreach (var stmt in _stmts)
                stmt.GenerateCode(lines, level);
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
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

        public void AddCode(string stmt)
            => AddCode(new CsSimpleStmt(stmt));

        public void AddCode(string cond, string line)
            => AddCode(new CsComplexStmt(cond, line));

        public void AddCode(CsStmt stmt)
            => Code.Add(stmt);

        public override D.CsStmt Concretize()
            => new D.CsComplexStmt(Before, Code.Select(x => x.Concretize()), After);
    }
}
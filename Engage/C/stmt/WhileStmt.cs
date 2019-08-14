using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class WhileStmt : CsStmt
    {
        public string Condition;
        public List<CsStmt> Code = new List<CsStmt>();

        public WhileStmt()
        {
        }

        public WhileStmt(string cond)
        {
            Condition = cond;
        }

        public WhileStmt(string cond, string stmt)
        {
            Condition = cond;
            Code.Add(new CsSimpleStmt(stmt));
        }

        public override D.CsStmt Concretize()
            => new D.CsComplexStmt($"while ({Condition})", Code.Select(x => x.Concretize()), "");
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class WhileStmt : CsStmt
    {
        public string Condition;
        public readonly bool Reversed = false;
        public readonly List<CsStmt> Code = new();

        public WhileStmt()
        {
        }

        public WhileStmt(string cond)
        {
            Condition = cond;
        }

        public WhileStmt(string cond, bool reversed)
        {
            Condition = cond;
            Reversed = reversed;
        }

        public WhileStmt(string cond, string stmt)
        {
            Condition = cond;
            AddCode(stmt);
        }

        public void AddCode(CsStmt stmt)
        {
            Code.Add(stmt);
        }

        public void AddCode(string stmt)
        {
            Code.Add(new SimpleStmt(stmt));
        }

        public override D.CsStmt Concretise()
            => Reversed
                ? new D.CsComplexStmt("do", Code.Select(x => x.Concretise()), $"while ({Condition})")
                : new D.CsComplexStmt($"while ({Condition})", Code.Select(x => x.Concretise()), "");
    }
}
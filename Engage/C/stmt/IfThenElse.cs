using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class IfThenElse : CsStmt
    {
        public SortedDictionary<string, List<CsStmt>> ThenBranches = new SortedDictionary<string, List<CsStmt>>();
        public List<CsStmt> ElseBranch = new List<CsStmt>();

        public IfThenElse()
        {
        }

        public IfThenElse(string cond1, string code1)
        {
            AddBranch(cond1, code1);
        }

        public IfThenElse(string cond1, string code1, string elsecode)
            : this(cond1, code1)
        {
            AddElse(elsecode);
        }

        public IfThenElse(string cond1, string code1, string cond2, string code2)
            : this(cond1, code1)
        {
            AddBranch(cond2, code2);
        }

        public IfThenElse(string cond1, string code1, string cond2, string code2, string elsecode)
           : this(cond1, code1, cond2, code2)
        {
            AddElse(elsecode);
        }

        public void AddBranch(string cond)
        {
            ThenBranches[cond] = new List<CsStmt>();
        }

        public void AddBranch(string cond, string code)
        {
            ThenBranches[cond] = new List<CsStmt>();
            ThenBranches[cond].Add(new CsSimpleStmt(code));
        }

        public void AddToBranch(string cond, string code)
        {
            if (!ThenBranches.ContainsKey(cond))
                ThenBranches[cond] = new List<CsStmt>();
            ThenBranches[cond].Add(new CsSimpleStmt(code));
        }

        public void AddToBranch(string cond, CsStmt code)
        {
            if (!ThenBranches.ContainsKey(cond))
                ThenBranches[cond] = new List<CsStmt>();
            ThenBranches[cond].Add(code);
        }

        public void AddElse(string code)
        {
            ElseBranch.Add(new CsSimpleStmt(code));
        }

        public override D.CsStmt Concretize()
        {
            var result = new D.CsStmtList();
            var kw = "if";
            foreach (var cond in ThenBranches.Keys)
            {
                var if1 = new D.CsComplexStmt();
                if1.Before = $"{kw} ({cond})";
                if1.Code.AddRange(ThenBranches[cond].Select(x => x.Concretize()));
                result.Stmts.Add(if1);
                kw = "else if";
            }
            if (ElseBranch.Count > 0)
            {
                var if2 = new D.CsComplexStmt();
                if2.Before = "else";
                if2.Code.AddRange(ElseBranch.Select(x => x.Concretize()));
                result.Stmts.Add(if2);
            }
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class IfThenElse : CsStmt
    {
        private List<string> Conditions = new List<string>();
        public Dictionary<string, List<CsStmt>> ThenBranches = new Dictionary<string, List<CsStmt>>();
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
            Conditions.Add(cond);
            ThenBranches[cond] = new List<CsStmt>();
        }

        public void AddBranch(string cond, string code)
            => AddBranch(cond, new SimpleStmt(code));

        public void AddBranch(string cond, CsStmt code)
        {
            Conditions.Add(cond);
            ThenBranches[cond] = new List<CsStmt>();
            ThenBranches[cond].Add(code);
        }

        public void AddBranch(string cond, IEnumerable<CsStmt> code)
        {
            Conditions.Add(cond);
            ThenBranches[cond] = new List<CsStmt>();
            ThenBranches[cond].AddRange(code);
        }

        public void AddToBranch(string cond, string code)
        {
            if (String.IsNullOrEmpty(cond))
                AddElse(code);
            else if (!ThenBranches.ContainsKey(cond))
                AddBranch(cond, code);
            else
                ThenBranches[cond].Add(new SimpleStmt(code));
        }

        public void AddToBranch(string cond, CsStmt code)
        {
            if (String.IsNullOrEmpty(cond))
                AddElse(code);
            else if (!ThenBranches.ContainsKey(cond))
                AddBranch(cond, code);
            else
                ThenBranches[cond].Add(code);
        }

        internal void AddToBranch(string cond, IEnumerable<CsStmt> code)
        {
            if (String.IsNullOrEmpty(cond))
                ElseBranch.AddRange(code);
            else if (!ThenBranches.ContainsKey(cond))
                AddBranch(cond, code);
            else
                ThenBranches[cond].AddRange(code);
        }

        public void RenameBranch(string oldname, string newname)
        {
            for(int i=0;i<Conditions.Count;i++)
            {
                if (Conditions[i]==oldname)
                {
                    Conditions[i] = newname;
                    ThenBranches[newname] = new List<CsStmt>();
                    AddToBranch(newname, ThenBranches[oldname]);
                    ThenBranches.Remove(oldname);
                }
            }
        }

        public void AddElse(string code)
        {
            ElseBranch.Add(new SimpleStmt(code));
        }

        public void AddElse(CsStmt code)
        {
            ElseBranch.Add(code);
        }

        public override D.CsStmt Concretize()
        {
            var result = new D.CsStmtList();
            var kw = "if";
            foreach (var cond in Conditions)
            {
                var if1 = new D.CsComplexStmt();
                if1.Before = $"{kw} ({cond})";
                if1.AddCode(ThenBranches[cond].Select(x => x.Concretize()));
                result.Stmts.Add(if1);
                kw = "else if";
            }
            if (ElseBranch.Count > 0)
            {
                var if2 = new D.CsComplexStmt();
                if2.Before = "else";
                if2.AddCode(ElseBranch.Select(x => x.Concretize()));
                result.Stmts.Add(if2);
            }
            return result;
        }
    }
}
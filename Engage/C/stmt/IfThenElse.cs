using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class IfThenElse : CsStmt
    {
        private readonly List<string> _conditions = new List<string>();
        private readonly Dictionary<string, List<CsStmt>> _thenBranches = new Dictionary<string, List<CsStmt>>();
        private readonly List<CsStmt> _elseBranch = new List<CsStmt>();

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

        private string NormaliseCondition(string cond)
        {
            if (cond == null)
                return cond;
            if (cond.StartsWith('_'))
                return cond;
            return cond.Replace("_", " && ");
        }

        public void AddBranch(string cond)
        {
            cond = NormaliseCondition(cond);
            _conditions.Add(cond);
            _thenBranches[cond] = new List<CsStmt>();
        }

        public void AddBranch(string cond, string code)
            => AddBranch(cond, new SimpleStmt(code));

        public void AddBranch(string cond, CsStmt code)
        {
            cond = NormaliseCondition(cond);
            _conditions.Add(cond);
            _thenBranches[cond] = new List<CsStmt> {code};
        }

        public void AddBranch(string cond, IEnumerable<CsStmt> code)
        {
            cond = NormaliseCondition(cond);
            _conditions.Add(cond);
            _thenBranches[cond] = new List<CsStmt>();
            _thenBranches[cond].AddRange(code);
        }

        public void AddToBranch(string cond, string code)
        {
            cond = NormaliseCondition(cond);
            if (String.IsNullOrEmpty(cond))
                AddElse(code);
            else if (!_thenBranches.ContainsKey(cond))
                AddBranch(cond, code);
            else
                _thenBranches[cond].Add(new SimpleStmt(code));
        }

        public void AddToBranch(string cond, CsStmt code)
        {
            cond = NormaliseCondition(cond);
            if (String.IsNullOrEmpty(cond))
                AddElse(code);
            else if (!_thenBranches.ContainsKey(cond))
                AddBranch(cond, code);
            else
                _thenBranches[cond].Add(code);
        }

        internal void AddToBranch(string cond, IEnumerable<CsStmt> code)
        {
            cond = NormaliseCondition(cond);
            if (String.IsNullOrEmpty(cond))
                _elseBranch.AddRange(code);
            else if (!_thenBranches.ContainsKey(cond))
                AddBranch(cond, code);
            else
                _thenBranches[cond].AddRange(code);
        }

        public void RenameBranch(string oldName, string newName)
        {
            oldName = NormaliseCondition(oldName);
            newName = NormaliseCondition(newName);
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i] != oldName) continue;
                _conditions[i] = newName;
                _thenBranches[newName] = new List<CsStmt>();
                AddToBranch(newName, _thenBranches[oldName]);
                _thenBranches.Remove(oldName);
            }
        }

        public List<CsStmt> GetThenBranch(string name)
            => _thenBranches[NormaliseCondition(name)];

        public string FirstThenBranchKey()
            => _thenBranches.Keys.First();

        public IEnumerable<CsStmt> FirstThenBranchValue()
            => _thenBranches.Values.First();

        public void AddElse(string code)
        {
            _elseBranch.Add(new SimpleStmt(code));
        }

        public void AddElse(CsStmt code)
        {
            _elseBranch.Add(code);
        }

        public override D.CsStmt Concretize()
        {
            var result = new D.CsStmtList();
            var kw = "if";
            foreach (var cond in _conditions)
            {
                var if1 = new D.CsComplexStmt
                {
                    Before = $"{kw} ({cond})"
                };
                if1.AddCode(_thenBranches[cond].Select(x => x.Concretize()));
                result.AddStmt(if1);
                kw = "else if";
            }

            if (_elseBranch.Count <= 0) return result;
            {
                var if2 = new D.CsComplexStmt
                {
                    Before = "else"
                };
                if2.AddCode(_elseBranch.Select(x => x.Concretize()));
                result.AddStmt(if2);
            }
            return result;
        }
    }
}
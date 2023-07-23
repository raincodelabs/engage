using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.GA
{
    public class IfThenElse : CsStmt
    {
        private readonly List<string> _conditions = new();
        private readonly Dictionary<string, List<CsStmt>> _thenBranches = new();
        internal readonly List<CsStmt> ElseBranch = new();

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
            if (cond == null || cond.StartsWith('_'))
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
            _thenBranches[cond] = new List<CsStmt> { code };
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
                ElseBranch.AddRange(code);
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
        {
            name = NormaliseCondition(name);
            if (!_thenBranches.ContainsKey(name))
                AddBranch(name);
            return _thenBranches[name];
        }

        public string FirstThenBranchKey()
            => _thenBranches.Keys.First();

        public IEnumerable<CsStmt> FirstThenBranchValue()
            => _thenBranches.Values.First();

        public void AddElse(string code)
        {
            ElseBranch.Add(new SimpleStmt(code));
        }

        public void AddElse(CsStmt code)
        {
            ElseBranch.Add(code);
        }

        public void AddElse(IEnumerable<CsStmt> codes)
        {
            ElseBranch.AddRange(codes);
        }

        public override GC.CsStmt Concretise()
        {
            var result = new GC.CsStmtList();
            var kw = "if";
            foreach (var cond in _conditions)
            {
                var if1 = new GC.CsComplexStmt
                {
                    Before = $"{kw} ({cond})"
                };
                if1.AddCode(_thenBranches[cond].Select(x => x.Concretise()));
                result.AddStmt(if1);
                kw = "else if";
            }

            // if there is no else, no need to generate code for it
            if (ElseBranch.Count == 0) return result;

            // if there is only one empty statement, we can consider it empty
            if (ElseBranch.Count == 1 &&
                ElseBranch[0] is GA.SimpleStmt elseSingleton &&
                String.IsNullOrEmpty(elseSingleton.Code))
                return result;

            var if2 = new GC.CsComplexStmt
            {
                Before = "else"
            };
            if2.AddCode(ElseBranch.Select(x => x.Concretise()));
            result.AddStmt(if2);
            return result;
        }
    }
}
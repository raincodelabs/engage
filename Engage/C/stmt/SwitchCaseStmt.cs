using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class SwitchCaseStmt : CsStmt
    {
        public string Expression;
        public readonly Dictionary<string, List<CsStmt>> Branches = new();
        public readonly List<CsStmt> DefaultBranch = new();

        public override D.CsStmt Concretise()
        {
            D.CsComplexStmt switch1 = new D.CsComplexStmt();
            switch1.Before = $"switch ({Expression})";

            var cases = Branches.Keys.ToList();
            cases.Sort((x, y) => y.Length - x.Length);

            foreach (var case1 in cases)
            {
                var case2 = new D.CsComplexStmt
                {
                    Embrace = false,
                    Before = $"case {case1}:"
                };
                // no curlies
                case2.AddCode(Branches[case1].Select(x => x.Concretise()));
                case2.AddCode("break");
                switch1.AddCode(case2);
            }

            // default
            if (DefaultBranch.Count <= 1) return switch1;

            var case3 = new D.CsComplexStmt
            {
                Embrace = false,
                Before = "default:"
            };
            // no curlies
            case3.AddCode(DefaultBranch.Select(x => x.Concretise()));
            case3.AddCode("break");
            switch1.AddCode(case3);

            return switch1;
        }
    }
}
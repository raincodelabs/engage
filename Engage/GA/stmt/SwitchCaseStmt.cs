using System.Collections.Generic;
using System.Linq;

namespace Engage.GA;

public class SwitchCaseStmt : CsStmt
{
    public string Expression;
    public readonly Dictionary<string, List<CsStmt>> Branches = new();
    private readonly List<CsStmt> _defaultBranch = new();

    public override GC.CsStmt Concretise()
    {
        GC.CsComplexStmt switch1 = new GC.CsComplexStmt();
        switch1.Before = $"switch ({Expression})";

        var cases = Branches.Keys.ToList();
        cases.Sort((x, y) => y.Length - x.Length);

        foreach (var case1 in cases)
        {
            var case2 = new GC.CsComplexStmt
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
        if (_defaultBranch.Count <= 1) return switch1;

        var case3 = new GC.CsComplexStmt
        {
            Embrace = false,
            Before = "default:"
        };
        // no curlies
        case3.AddCode(_defaultBranch.Select(x => x.Concretise()));
        case3.AddCode("break");
        switch1.AddCode(case3);

        return switch1;
    }
}
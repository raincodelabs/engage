using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class CsSwitchCase : CsStmt
    {
        public string Expression;
        public Dictionary<string, List<CsStmt>> Branches = new Dictionary<string, List<CsStmt>>();
        public List<CsStmt> DefaultBranch = new List<CsStmt>();

        public override D.CsStmt Concretize()
        {
            Dictionary<string, List<D.CsStmt>> DBranches = new Dictionary<string, List<D.CsStmt>>();
            foreach (var k in Branches.Keys)
            {
                DBranches[k] = new List<D.CsStmt>();
                Branches[k].ForEach(x => DBranches[k].Add(x.Concretize()));
            }
            return new D.CsSwitchCase(Expression, DBranches, DefaultBranch.Select(x => x.Concretize()));
        }
    }
}
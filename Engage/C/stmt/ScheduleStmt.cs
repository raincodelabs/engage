using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class ScheduleStmt : CsStmt
    {
        public string Type;
        public string Var;
        public List<CsStmt> Code = new List<CsStmt>();

        public ScheduleStmt()
        {
        }

        public ScheduleStmt(string type, string var)
        {
            Type = type;
            Var = var;
        }

        public void AddCode(string stmt)
        {
            Code.Add(new SimpleStmt(stmt));
        }

        public void AddCode(CsStmt stmt)
        {
            Code.Add(stmt);
        }

        public override D.CsStmt Concretize()
        {
            var lambda = new D.CsComplexStmt();
            lambda.Before = $"Schedule(typeof({Type}), {Var} =>";
            lambda.After = ");";
            lambda.AddCode(Code.Select(x => x.Concretize()));
            return lambda;
        }
    }
}
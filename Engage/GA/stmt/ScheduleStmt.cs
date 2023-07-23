using System.Collections.Generic;
using System.Linq;

namespace Engage.GA;

public class ScheduleStmt : CsStmt
{
    private readonly string _type;
    private readonly string _var;
    public List<CsStmt> Code { get; } = new();

    public ScheduleStmt()
    {
    }

    public ScheduleStmt(string type, string var)
    {
        _type = type;
        _var = var;
    }

    public void AddCode(string stmt)
    {
        Code.Add(new SimpleStmt(stmt));
    }

    public void AddCode(CsStmt stmt)
    {
        Code.Add(stmt);
    }

    public override GC.CsStmt Concretise()
    {
        var lambda = new GC.CsComplexStmt
        {
            Before = $"Schedule(typeof({_type}), {_var} =>",
            After = ");"
        };
        lambda.AddCode(Code.Select(x => x.Concretise()));
        return lambda;
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Engage.GA;

public class WhileStmt : CsStmt
{
    public string Condition;
    private readonly bool _reversed;
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
        _reversed = reversed;
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

    public override GC.CsStmt Concretise()
        => _reversed
            ? new GC.CsComplexStmt("do", Code.Select(x => x.Concretise()), $"while ({Condition})")
            : new GC.CsComplexStmt($"while ({Condition})", Code.Select(x => x.Concretise()), "");
}
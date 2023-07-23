using System;
using System.Collections.Generic;

namespace Engage.GA;

public abstract class CsExeField
{
    public bool IsPublic { get; init; } = true;
    protected readonly List<Tuple<string, string>> Args = new();
    protected readonly List<CsStmt> Code = new();

    public abstract GC.CsExeField Concretise();

    public void AddArgument(string name, string type)
    {
        Args.Add(new Tuple<string, string>(name, type));
    }

    public void AddCode(string line)
        => AddCode(new SimpleStmt(line));

    public void AddCode(CsStmt line)
    {
        if (line != null)
            Code.Add(line);
    }
}
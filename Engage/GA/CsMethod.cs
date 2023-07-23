using System.Linq;

namespace Engage.GA;

public class CsMethod : CsExeField
{
    public string Name;
    public string RetType;

    public override GC.CsExeField Concretise()
        => new GC.CsMethod(Name, RetType, IsPublic, Args, Code.Select(c => c.Concretise()));
}
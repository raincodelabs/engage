using System.Collections.Generic;

namespace Engage.GA;

public class CsEnum : CsTop
{
    public bool IsPublic { get; init; } = true;
    private readonly List<string> _values = new();

    public override GC.CsTop Concretise()
        => new GC.CsEnum(Name, IsPublic, _values);

    internal void Add(string v)
        => _values.Add(v);

    internal void Add(IEnumerable<string> vs)
        => _values.AddRange(vs);
}
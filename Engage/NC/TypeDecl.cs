using System.Collections.Generic;
using System.Linq;

namespace Engage.NC;

public class TypeDecl
{
    public readonly List<string> Names = new();
    public string Super = "";

    public override bool Equals(object obj)
    {
        var other = obj as TypeDecl;
        if (other == null)
            return false;
        return Super == other.Super
               && Names.Count == other.Names.Count
               && Names.SequenceEqual(other.Names)
            ;
    }

    public override int GetHashCode()
        => Super.GetHashCode()
           + Names.Select(name => name.GetHashCode()).Sum();
}
using System.Collections.Generic;
using System.Linq;

namespace Engage.NC
{
    public class TypeDecl
    {
        public List<string> Names = new List<string>();
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
    }
}
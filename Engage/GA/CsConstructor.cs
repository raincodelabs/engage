using System.Linq;

namespace Engage.GA
{
    public class CsConstructor : CsExeField
    {
        public bool InheritFromBase = false;

        public override GC.CsExeField Concretise()
            => new GC.CsConstructor(InheritFromBase, IsPublic, Args, Code.Select(c => c.Concretise()));
    }
}
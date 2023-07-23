using System.Linq;

namespace Engage.C
{
    public class CsConstructor : CsExeField
    {
        public bool InheritFromBase = false;

        public override D.CsExeField Concretise()
            => new D.CsConstructor(InheritFromBase, IsPublic, Args, Code.Select(c => c.Concretise()));
    }
}
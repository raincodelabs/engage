using System.Linq;

namespace Engage.C
{
    public class CsConstructor : CsExeField
    {
        public bool InheritFromBase = false;

        public override D.CsExeField Concretize()
            => new D.CsConstructor(InheritFromBase, IsPublic, Args, Code.Select(c => c.Concretize()));
    }
}
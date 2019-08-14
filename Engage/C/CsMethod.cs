using System.Linq;

namespace Engage.C
{
    public class CsMethod : CsExeField
    {
        public string Name;
        public string RetType;

        public override D.CsExeField Concretize()
            => new D.CsMethod(Name, RetType, IsPublic, Args, Code.Select(c => c.Concretize()));
    }
}
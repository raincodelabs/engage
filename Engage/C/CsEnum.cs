using System.Collections.Generic;

namespace Engage.C
{
    public class CsEnum : CsTop
    {
        public bool IsPublic = true;
        public List<string> Values = new List<string>();

        public override D.CsTop Concretize()
            => new D.CsEnum(Name, IsPublic, Values);
    }
}
using System.Collections.Generic;

namespace Engage.C
{
    public class CsEnum : CsTop
    {
        public bool IsPublic = true;
        private List<string> Values = new List<string>();

        public override D.CsTop Concretize()
            => new D.CsEnum(Name, IsPublic, Values);

        internal void Add(string v)
            => Values.Add(v);

        internal void Add(IEnumerable<string> vs)
            => Values.AddRange(vs);
    }
}
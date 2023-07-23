using System.Collections.Generic;

namespace Engage.C
{
    public class CsEnum : CsTop
    {
        public bool IsPublic = true;
        private readonly List<string> _values = new();

        public override D.CsTop Concretise()
            => new D.CsEnum(Name, IsPublic, _values);

        internal void Add(string v)
            => _values.Add(v);

        internal void Add(IEnumerable<string> vs)
            => _values.AddRange(vs);
    }
}
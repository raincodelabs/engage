using System.Collections.Generic;

namespace Engage.D
{
    public class CsEnum : CsTop
    {
        public readonly bool IsPublic;
        private readonly List<string> _values;

        public CsEnum(string name, bool isPublic, List<string> values)
        {
            Name = name;
            IsPublic = isPublic;
            _values = values;
        }

        public override void GenerateCode(List<string> lines, int level)
        {
            lines.Add(level, $"{(IsPublic ? "public" : "private")} enum {Name}");
            lines.Open(level);
            foreach (var v in _values)
                lines.Add(level + 1, v + ",");
            lines.Close(level);
        }

        internal void Add(string v)
            => _values.Add(v);

        internal void Add(IEnumerable<string> vs)
            => _values.AddRange(vs);
    }
}
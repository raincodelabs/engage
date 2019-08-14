using System.Collections.Generic;

namespace Engage.D
{
    public class CsEnum : CsTop
    {
        public bool IsPublic = true;
        public List<string> Values = new List<string>();

        public CsEnum(string name, bool isPublic, List<string> values)
        {
            Name = name;
            IsPublic = isPublic;
            Values = values;
        }

        public override void GenerateCode(List<string> lines, int level)
        {
            lines.Add(level, $"{(IsPublic ? "public" : "private")} enum {Name}");
            lines.Open(level);
            foreach (var v in Values)
                lines.Add(level + 1, v + ",");
            lines.Close(level);
        }
    }
}
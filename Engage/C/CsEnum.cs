using System.Collections.Generic;

namespace Engage.C
{
    public class CsEnum : CsTop
    {
        public bool IsPublic = true;
        public List<string> Values = new List<string>();

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
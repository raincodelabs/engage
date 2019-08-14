using System.Collections.Generic;

namespace Engage.D
{
    public class CsSimpleStmt : CsStmt
    {
        public string Code;

        public CsSimpleStmt()
        {
        }

        public CsSimpleStmt(string code)
        {
            Code = code;
        }

        public override void GenerateCode(List<string> lines, int level)
        {
            if (!Code.EndsWith(";"))
                Code += ";";
            lines.Add(level, Code);
        }
    }
}
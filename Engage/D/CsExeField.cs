using System;
using System.Collections.Generic;

namespace Engage.D
{
    public abstract class CsExeField
    {
        public bool IsPublic = true;
        protected List<Tuple<string, string>> Args = new List<Tuple<string, string>>();
        protected List<CsStmt> Code = new List<CsStmt>();

        public void AddArgument(string name, string type)
        {
            Args.Add(new Tuple<string, string>(name, type));
        }

        public void AddCode(string line)
            => AddCode(new CsSimpleStmt(line));

        public void AddCode(string cond, string line)
            => AddCode(new CsComplexStmt(cond, line));

        public void AddCode(CsStmt line)
        {
            if (line != null)
                Code.Add(line);
        }

        public abstract void GenerateCode(List<string> lines, int level, string className);
    }
}
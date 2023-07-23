﻿using System;
using System.Collections.Generic;

namespace Engage.C
{
    public abstract class CsExeField
    {
        public bool IsPublic = true;
        protected readonly List<Tuple<string, string>> Args = new();
        protected readonly List<CsStmt> Code = new();

        public abstract D.CsExeField Concretise();

        public void AddArgument(string name, string type)
        {
            Args.Add(new Tuple<string, string>(name, type));
        }

        public void AddCode(string line)
            => AddCode(new SimpleStmt(line));

        public void AddCode(CsStmt line)
        {
            if (line != null)
                Code.Add(line);
        }
    }
}
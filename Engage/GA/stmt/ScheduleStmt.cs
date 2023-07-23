﻿using System.Collections.Generic;
using System.Linq;

namespace Engage.GA
{
    public class ScheduleStmt : CsStmt
    {
        public string Type;
        public string Var;
        public readonly List<CsStmt> Code = new();

        public ScheduleStmt()
        {
        }

        public ScheduleStmt(string type, string var)
        {
            Type = type;
            Var = var;
        }

        public void AddCode(string stmt)
        {
            Code.Add(new SimpleStmt(stmt));
        }

        public void AddCode(CsStmt stmt)
        {
            Code.Add(stmt);
        }

        public override GC.CsStmt Concretise()
        {
            var lambda = new GC.CsComplexStmt
            {
                Before = $"Schedule(typeof({Type}), {Var} =>",
                After = ");"
            };
            lambda.AddCode(Code.Select(x => x.Concretise()));
            return lambda;
        }
    }
}
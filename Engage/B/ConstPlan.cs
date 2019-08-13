using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.B
{
    public class ConstPlan
    {
        public List<Tuple<string, TypePlan>> Args = new List<Tuple<string, TypePlan>>();

        public string ToString(string name, string super)
        {
            string result = name;
            if (!String.IsNullOrEmpty(super))
                result += ":" + super;
            if (Args.Count > 0)
            {
                result += "(";
                result += String.Join(",", Args.Select(a => $"{a.Item1}:{(a.Item2 == null ? "object" : a.Item2.ToString())}"));
                result += ")";
            }
            return result;
        }
    }
}
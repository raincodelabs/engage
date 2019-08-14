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

        public override bool Equals(object obj)
        {
            var other = obj as ConstPlan;
            if (other == null)
                return false;
            if (this.Args.Count != other.Args.Count)
                return false;
            for (int i = 0; i < Args.Count; i++)
                if (this.Args[i].Item1 != other.Args[i].Item1 || this.Args[i].Item2 != this.Args[i].Item2)
                    return false;
            return true;
        }
    }
}
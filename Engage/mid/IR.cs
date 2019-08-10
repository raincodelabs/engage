using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.mid
{
    public class SystemPlan
    {
        public Dictionary<string, TypePlan> Types = new Dictionary<string, TypePlan>();

        internal void AddType(string n, string super, bool silent = false)
        {
            if (String.IsNullOrEmpty(n))
                return;
            if (Types.ContainsKey(n))
            {
                if (!silent)
                    Console.WriteLine($"[IR] Cannot add type '{n}' the second time");
                return;
            }
            TypePlan tp = new TypePlan();
            tp.Name = n;
            tp.Super = super;
            Console.WriteLine($"[IR] Added type '{n}' to the plan");
            Types[tp.Name] = tp;
        }

        internal bool IsType(string n) => Types.ContainsKey(n);
    }

    public class TypePlan
    {
        public string Name;
        public string Super;
        public bool IsList = false;
        public List<ConstPlan> Constructors = new List<ConstPlan>();

        public TypePlan Copy(bool turnIntoList = false)
        {
            TypePlan plan = new TypePlan();
            plan.Name = Name;
            plan.Super = Super;
            plan.IsList = IsList || turnIntoList;
            // do not copy constructors!
            return plan;
        }
    }

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
                result += String.Join(",", Args.Select(a=> $"{a.Item1}:{a.Item2.Name}"));
                result += ")";
            }
            return result;
        }
    }
}
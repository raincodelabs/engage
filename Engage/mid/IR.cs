using Engage.back;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.mid
{
    public class SystemPlan
    {
        public string NS;
        public Dictionary<string, TypePlan> Types = new Dictionary<string, TypePlan>();
        public HashSet<string> BoolFlags = new HashSet<string>();
        public HashSet<string> IntFlags = new HashSet<string>();

        public SystemPlan(string ns)
        {
            NS = ns;
        }

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

        public IEnumerable<CsClass> GenerateDataClasses()
            => Types.Values
                .Where(t => !t.IsList)
                .Select(t => t.GenerateClass(NS));

        public CsClass GenerateParser()
        {
            var p = new CsClass();
            p.NS = NS;
            p.Name = "Parser";
            p.AddUsing("System");
            if (BoolFlags.Count > 0)
                p.AddField(String.Join(", ", BoolFlags), "bool", isPublic: false);
            if (IntFlags.Count > 0)
                p.AddField(String.Join(", ", IntFlags), "int", isPublic: false);
            p.AddField("Main", "Stack<Object>", isPublic: false);
            p.AddField("_input", "string", isPublic: false);
            p.AddField("_pos", "int", isPublic: false);
            return p;
        }
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

        public override string ToString()
            => IsList ? $"List<{Name}>" : Name;

        internal CsClass GenerateClass(string ns)
        {
            var result = new CsClass();
            result.NS = ns;
            result.Name = Name;
            result.Super = Super;
            foreach (var c in Constructors)
            {
                var cc = new CsConstructor();
                foreach (var a in c.Args)
                {
                    result.AddField(a.Item1, a.Item2.ToString());
                    cc.AddArgument(a.Item1, a.Item2.ToString());
                }
                result.AddConstructor(cc);
            }
            return result;
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
                result += String.Join(",", Args.Select(a => $"{a.Item1}:{a.Item2.ToString()}"));
                result += ")";
            }
            return result;
        }
    }
}
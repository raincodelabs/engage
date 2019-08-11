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
            p.AddField("input", "string", isPublic: false);
            p.AddField("pos", "int", isPublic: false);
            // parser constructor
            var pc = new CsConstructor();
            pc.AddArgument("input", "string");
            pc.AddCode("pos = 0");
            p.AddConstructor(pc);
            // the parse function
            var pf = new CsMethod();
            pf.Name = "Parse";
            pf.RetType = "object";
            pf.AddCode("TokenType type");
            pf.AddCode("string lexeme");
            List<CsStmt> loop = new List<CsStmt>();
            var pl = new CsComplexStmt();
            pl.Before = "do";
            pl.After = "while (type != TokenType.TEOF)";

            // main parsing loop: begin
            pl.AddCode("var t = NextToken();");
            pl.AddCode("lexeme = t.Item2;");
            pl.AddCode("type = t.Item1;");
            // main parsing loop: end

            pf.AddCode(pl);
            pf.AddCode("return null"); // TODO!!!
            p.AddMethod(pf);

            // tokeniser
            var tok = new CsMethod();
            tok.IsPublic = false;
            tok.Name = "NextToken";
            tok.RetType = "Tuple<TokenType, string>";
            tok.AddCode("return null"); // TODO
            p.AddMethod(tok);

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
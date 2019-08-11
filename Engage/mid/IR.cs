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
        public Dictionary<string, List<TokenPlan>> Tokens = new Dictionary<string, List<TokenPlan>>();

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
            // token types
            var tt = new CsEnum();
            tt.IsPublic = false;
            tt.Name = "TokenType";
            tt.Values.Add("TUndefined");
            tt.Values.Add("TEOF");
            tt.Values.AddRange(Tokens.Keys.Where(t => t != "skip").Select(t => "T" + t));
            p.AddInner(tt);
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
            GenerateTokeniser(tok);
            p.AddMethod(tok);

            return p;
        }

        private void GenerateTokeniser(CsMethod tok)
        {
            // init phase
            tok.AddCode("TokenType t = TokenType.TUndefined;");
            tok.AddCode("string s = \"\";");
            // EOF phase
            tok.AddCode(new CsComplexStmt("if (pos >= input.Length)", "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")"));
            // skip
            if (Tokens.ContainsKey("skip"))
            {
                string cond = String.Join(" || ", Tokens["skip"].Select(t => $"input[pos] == '{t.Value}'"));
                tok.AddCode(new CsComplexStmt($"while ({cond} && pos < input.Length)", "pos++"));
            }
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'skip'");
            // EOF after skip
            tok.AddCode(new CsComplexStmt("if (pos >= input.Length)", "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")"));
            // reserved
            if (Tokens.ContainsKey("reserved"))
                GenerateBranches("reserved", tok);
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'reserved'");
            // number
            foreach (var tt in Tokens.Keys)
            {
                if (tt == "skip")
                    continue;
                if (tt == "reserved")
                    continue;
                GenerateBranches(tt, tok);
            }
            tok.AddCode("return new Tuple<TokenType, string>(t, s);");
        }

        // Precondition: Tokens.Contains(token_name)
        private void GenerateBranches(string token_name, CsMethod method)
        {
            if (!Tokens.ContainsKey(token_name))
                return;
            foreach (var tm in Tokens[token_name])
                if (tm.Special)
                    method.AddCode(GenerateBranchSpecialMatch(tm.Value, token_name));
                else
                    method.AddCode(GenerateBranchPreciseMatch(tm.Value, token_name));
        }

        private CsComplexStmt GenerateBranchSpecialMatch(string value, string type)
        {
            switch (value)
            {
                case "number":
                    return GenerateBranchNumberMatch(type);

                case "string":
                    return GenerateBranchStringMatch(type);

                default:
                    Console.WriteLine($"[IR] Cannot generate a match for '{value}'");
                    return null;
            }
        }

        private CsComplexStmt GenerateBranchStringMatch(string type)
        {
            string cond = "";
            if (Tokens.ContainsKey("skip"))
                foreach (var t in Tokens["skip"])
                    cond += $" && input[pos] != '{t.Value}'";
            CsComplexStmt ifst = new CsComplexStmt();
            ifst.Before = "else";
            ifst.AddCode($"t = TokenType.T{type}");
            ifst.AddCode($"while (pos < input.Length{cond})", "s += input[pos++]");
            return ifst;
        }

        private CsComplexStmt GenerateBranchNumberMatch(string type)
        {
            CsComplexStmt ifst = new CsComplexStmt();
            string cond = string.Join(" || ", "0123456789".Select(c => $"input[pos] == '{c}'"));
            ifst.Before = $"else if ({cond})";
            ifst.AddCode($"t = TokenType.T{type}");
            ifst.AddCode($"while (pos < input.Length && ({cond}))", "s += input[pos++]");
            return ifst;
        }

        private CsComplexStmt GenerateBranchPreciseMatch(string value, string type)
        {
            int len = value.Length;
            CsComplexStmt ifst = new CsComplexStmt();
            string cond;
            if (len > 1)
                cond = $"pos + {len - 1} < input.Length";
            else
                cond = "";
            for (int i = 0; i < len; i++)
                cond += $" && input[pos + {i}] == '{value[i]}'";
            if (cond.StartsWith(" && "))
                cond = cond.Substring(4);
            cond = cond.Replace(" + 0", "");
            ifst.Before = $"else if ({cond})";
            ifst.AddCode($"t = TokenType.T{type}");
            ifst.AddCode($"s = \"{value}\"");
            if (len > 1)
                ifst.AddCode($"pos += {len}");
            else
                ifst.AddCode("pos++");
            return ifst;
        }
    }

    public class TokenPlan
    {
        public bool Special = false;
        public string Value;
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
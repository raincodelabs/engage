using Engage.back;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.mid
{
    public class SystemPlan
    {
        public string NS;
        public string TopType;
        public Dictionary<string, TypePlan> Types = new Dictionary<string, TypePlan>();
        public HashSet<string> BoolFlags = new HashSet<string>();
        public HashSet<string> IntFlags = new HashSet<string>();
        public Dictionary<string, List<TokenPlan>> Tokens = new Dictionary<string, List<TokenPlan>>();
        public Dictionary<string, List<HandlerPlan>> Handlers = new Dictionary<string, List<HandlerPlan>>();
        //public List<HandlerPlan> Handlers = new List<HandlerPlan>();

        public SystemPlan(string ns)
        {
            NS = ns;
        }

        internal void AddType(string n, string super, bool silent = false)
        {
            if (String.IsNullOrEmpty(n))
                return;
            if (Types.Count == 0)
            {
                TopType = n;
                Console.WriteLine($"[IR] Top type is assumed to be {TopType}");
            }
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
            p.AddField("Pending", "Dictionary<System.Type, Queue<Action<object>>>", isPublic: false);
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
            pf.AddCode("string ERROR = \"\"");
            pf.AddCode("TokenType type");
            pf.AddCode("string lexeme");
            List<CsStmt> loop = new List<CsStmt>();
            var pl = new CsComplexStmt();
            pl.Before = "do";
            pl.After = "while (type != TokenType.TEOF)";

            // main parsing loop: begin
            pl.AddCode("var _token = NextToken();");
            pl.AddCode("lexeme = _token.Item2;");
            pl.AddCode("type = _token.Item1;");

            var swType = new CsSwitchCase();
            swType.Expression = "type";

            foreach (var hpk in Handlers.Keys)
            {
                List<CsStmt> branchType = new List<CsStmt>();
                if (Handlers[hpk].Count == 1)
                {
                    foreach (var action in Handlers[hpk][0].Recipe)
                    {
                        if (action != null)
                            action.GenerateAbstractCode(branchType);
                        else
                            Console.WriteLine($"[IR] Warning: no action to handle '{hpk}'/{Handlers[hpk][0].ReactOn.Value}");
                    }
                }
                else
                {
                    var swLex = new CsSwitchCase();
                    swLex.Expression = "lexeme";
                    foreach (var hp in Handlers[hpk])
                    {
                        List<CsStmt> branchLex = new List<CsStmt>();
                        //Console.WriteLine($"[IR] in '{hpk}', handle {hp.ReactOn.Value}");
                        foreach (var action in hp.Recipe)
                        {
                            if (action != null)
                                action.GenerateAbstractCode(branchLex);
                            else
                                Console.WriteLine($"[IR] Warning: no action to handle '{hpk}'/{hp.ReactOn.Value}");
                        }
                        swLex.Branches['"' + hp.ReactOn.Value + '"'] = branchLex;
                    }
                    branchType.Add(swLex);
                }
                swType.Branches["TokenType.T" + hpk] = branchType;
            }

            pl.AddCode(swType);
            var abend = new CsComplexStmt();
            abend.Before = "if (!System.String.IsNullOrEmpty(ERROR))";
            abend.AddCode("Console.WriteLine(\"Parser error: \" + ERROR);");
            abend.AddCode("return null;");
            pl.AddCode(abend);
            // main parsing loop: end

            pf.AddCode(pl);
            pf.AddCode("return null"); // TODO!!!
            p.AddMethod(pf);

            // other methods
            GenerateAsync(p);
            GeneratePusher(p);
            GenerateTokeniser(p);

            return p;
        }

        private void GenerateAsync(CsClass cls)
        {
            var wait = new CsMethod();
            wait.IsPublic = false;
            wait.Name = "LetWait";
            wait.RetType = "void";
            wait.AddArgument("type", "System.Type");
            wait.AddArgument("action", "Action<object>");
            var ifst = new CsComplexStmt("if (Main.Peek().GetType() == _type)", "_action(Main.Pop());");
            ifst.AddCode("return");
            wait.AddCode(ifst);
            wait.AddCode("if (!Pending.ContainsKey(_type))", "Pending[_type] = new Queue<Action<object>>();");
            wait.AddCode("Pending[_type].Enqueue(_action)");
            cls.AddMethod(wait);
        }

        private void GeneratePusher(CsClass cls)
        {
            var push = new CsMethod();
            push.IsPublic = false;
            push.Name = "Push";
            push.RetType = "void";
            push.AddArgument("x", "object");
            push.AddCode("System.Type _t = _x.GetType()");
            var ifst = new CsComplexStmt("if (Pending.ContainsKey(_t) && Pending[_t].Count > 0)", "Action<object> _a = Pending[_t].Dequeue();");
            ifst.AddCode("_a(_x)");
            push.AddCode(ifst);
            push.AddCode("else", "Main.Push(_x)");
            cls.AddMethod(push);
        }

        private void GenerateTokeniser(CsClass cls)
        {
            var tok = new CsMethod();
            tok.IsPublic = false;
            tok.Name = "NextToken";
            tok.RetType = "Tuple<TokenType, string>";

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

            cls.AddMethod(tok);
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

        public override bool Equals(object obj)
        {
            if (obj is TokenPlan other)
                return other.Value == this.Value && other.Special == this.Special;
            return false;
        }

        public override int GetHashCode()
            => Value.GetHashCode() + Special.GetHashCode();
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

    public class HandlerPlan
    {
        public TokenPlan ReactOn;
        public string GuardFlag;
        public List<HandleAction> Recipe = new List<HandleAction>();
    }

    public abstract class HandleAction
    {
        public abstract void GenerateAbstractCode(List<CsStmt> code);
    }

    public class LiftFlag : HandleAction
    {
        public string Flag;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            if (Flag.EndsWith('#'))
                code.Add(new CsSimpleStmt($"{Flag.Substring(0, Flag.Length - 1)}++"));
            else
                code.Add(new CsSimpleStmt($"{Flag} = true"));
        }
    }

    public class DropFlag : HandleAction
    {
        public string Flag;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            if (Flag.EndsWith('#'))
                code.Add(new CsSimpleStmt($"{Flag.Substring(0, Flag.Length - 1)}--"));
            else
                code.Add(new CsSimpleStmt($"{Flag} = false"));
        }
    }

    public class PushNew : HandleAction
    {
        public string Name;
        public List<string> Args = new List<string>();

        public PushNew()
        {
        }

        public PushNew(string name, IEnumerable<string> args)
        {
            Name = name;
            Args.AddRange(args);
        }

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"Push(new {Name}({String.Join(", ", Args)}))"));
        }
    }

    public class PopOne : HandleAction
    {
        public string Name;
        public string Target;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"{Name} {Target}"));
            var tmp = new CsComplexStmt($"if (Main.Peek() is {Name})", $"{Target} = Main.Pop() as {Name}");
            code.Add(tmp);
            tmp = new CsComplexStmt($"else", $"ERROR = \"the top of the stack is not of type {Name}\"");
            tmp.AddCode($"{Target} = null");
            code.Add(tmp);
        }
    }

    public class PopAll : HandleAction
    {
        public string Name;
        public string Target;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"var {Target} = new List<{Name}>()"));
            var tmp = new CsComplexStmt($"while (Main.Peek() is {Name})", $"{Target}.Add(Main.Pop() as {Name})");
            code.Add(tmp);
        }
    }

    public class AwaitOne : HandleAction
    {
        public string Name;
        public string Flag;
        public string Target;
        public string ExtraFlag;

        public HandleAction BaseAction = null;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            HandleAction tmp;
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new LiftFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(code);
            }
            // TODO: check for "extra flag"
            var lambda = new CsComplexStmt();
            lambda.Before = $"LetWait(typeof({Name}), _{Target} =>";
            lambda.After = ");";
            lambda.AddCode($"var {Target} = _{Target} as {Name};");
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new DropFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(lambda.Code);
            }
            if (!String.IsNullOrEmpty(ExtraFlag))
                lambda.AddCode($"if (!{ExtraFlag})", $"ERROR = \"flag {ExtraFlag} was not raised when expected\"");
            if (BaseAction != null)
                BaseAction.GenerateAbstractCode(lambda.Code);
            code.Add(lambda);
        }
    }
}
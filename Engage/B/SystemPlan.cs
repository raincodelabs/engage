﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.B
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

        public IEnumerable<C.CsClass> GenerateDataClasses()
            => Types.Values
                .Where(t => !t.IsList)
                .Select(t => t.GenerateClass(NS));

        public C.CsClass GenerateParser()
        {
            var p = new C.CsClass();
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
            var tt = new C.CsEnum();
            tt.IsPublic = false;
            tt.Name = "TokenType";
            tt.Values.Add("TUndefined");
            tt.Values.Add("TEOF");
            tt.Values.AddRange(Tokens.Keys.Where(t => t != "skip").Select(t => "T" + t));
            p.AddInner(tt);
            // parser constructor
            var pc = new C.CsConstructor();
            pc.AddArgument("input", "string");
            pc.AddCode("pos = 0");
            p.AddConstructor(pc);
            // the parse function
            var pf = new C.CsMethod();
            pf.Name = "Parse";
            pf.RetType = "object";
            pf.AddCode("string ERROR = \"\"");
            pf.AddCode("TokenType type");
            pf.AddCode("string lexeme");
            List<C.CsStmt> loop = new List<C.CsStmt>();
            var pl = new C.CsComplexStmt();
            pl.Before = "do";
            pl.After = "while (type != TokenType.TEOF)";

            // main parsing loop: begin
            pl.AddCode("var _token = NextToken();");
            pl.AddCode("lexeme = _token.Item2;");
            pl.AddCode("type = _token.Item1;");

            var swType = new C.CsSwitchCase();
            swType.Expression = "type";

            foreach (var hpk in Handlers.Keys)
            {
                List<C.CsStmt> branchType = new List<C.CsStmt>();
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
                    var swLex = new C.CsSwitchCase();
                    swLex.Expression = "lexeme";
                    foreach (var hp in Handlers[hpk])
                    {
                        List<C.CsStmt> branchLex = new List<C.CsStmt>();
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
            var abend = new C.CsComplexStmt();
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

        private void GenerateAsync(C.CsClass cls)
        {
            var wait = new C.CsMethod();
            wait.IsPublic = false;
            wait.Name = "LetWait";
            wait.RetType = "void";
            wait.AddArgument("type", "System.Type");
            wait.AddArgument("action", "Action<object>");
            var ifst = new C.CsComplexStmt("if (Main.Peek().GetType() == _type)", "_action(Main.Pop());");
            ifst.AddCode("return");
            wait.AddCode(ifst);
            wait.AddCode("if (!Pending.ContainsKey(_type))", "Pending[_type] = new Queue<Action<object>>();");
            wait.AddCode("Pending[_type].Enqueue(_action)");
            cls.AddMethod(wait);
        }

        private void GeneratePusher(C.CsClass cls)
        {
            var push = new C.CsMethod();
            push.IsPublic = false;
            push.Name = "Push";
            push.RetType = "void";
            push.AddArgument("x", "object");
            push.AddCode("System.Type _t = _x.GetType()");
            var ifst = new C.CsComplexStmt("if (Pending.ContainsKey(_t) && Pending[_t].Count > 0)", "Action<object> _a = Pending[_t].Dequeue();");
            ifst.AddCode("_a(_x)");
            push.AddCode(ifst);
            push.AddCode("else", "Main.Push(_x)");
            cls.AddMethod(push);
        }

        private void GenerateTokeniser(C.CsClass cls)
        {
            var tok = new C.CsMethod();
            tok.IsPublic = false;
            tok.Name = "NextToken";
            tok.RetType = "Tuple<TokenType, string>";

            // init phase
            tok.AddCode("TokenType t = TokenType.TUndefined;");
            tok.AddCode("string s = \"\";");
            // EOF phase
            tok.AddCode(new C.CsComplexStmt("if (pos >= input.Length)", "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")"));
            // skip
            if (Tokens.ContainsKey("skip"))
            {
                string cond = String.Join(" || ", Tokens["skip"].Select(t => $"input[pos] == '{t.Value}'"));
                tok.AddCode(new C.CsComplexStmt($"while ({cond} && pos < input.Length)", "pos++"));
            }
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'skip'");
            // EOF after skip
            tok.AddCode(new C.CsComplexStmt("if (pos >= input.Length)", "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")"));
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
        private void GenerateBranches(string token_name, C.CsMethod method)
        {
            if (!Tokens.ContainsKey(token_name))
                return;
            foreach (var tm in Tokens[token_name])
                if (tm.Special)
                    method.AddCode(GenerateBranchSpecialMatch(tm.Value, token_name));
                else
                    method.AddCode(GenerateBranchPreciseMatch(tm.Value, token_name));
        }

        private C.CsComplexStmt GenerateBranchSpecialMatch(string value, string type)
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

        private C.CsComplexStmt GenerateBranchStringMatch(string type)
        {
            string cond = "";
            if (Tokens.ContainsKey("skip"))
                foreach (var t in Tokens["skip"])
                    cond += $" && input[pos] != '{t.Value}'";
            C.CsComplexStmt ifst = new C.CsComplexStmt();
            ifst.Before = "else";
            ifst.AddCode($"t = TokenType.T{type}");
            ifst.AddCode($"while (pos < input.Length{cond})", "s += input[pos++]");
            return ifst;
        }

        private C.CsComplexStmt GenerateBranchNumberMatch(string type)
        {
            C.CsComplexStmt ifst = new C.CsComplexStmt();
            string cond = string.Join(" || ", "0123456789".Select(c => $"input[pos] == '{c}'"));
            ifst.Before = $"else if ({cond})";
            ifst.AddCode($"t = TokenType.T{type}");
            ifst.AddCode($"while (pos < input.Length && ({cond}))", "s += input[pos++]");
            return ifst;
        }

        private C.CsComplexStmt GenerateBranchPreciseMatch(string value, string type)
        {
            int len = value.Length;
            C.CsComplexStmt ifst = new C.CsComplexStmt();
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
}
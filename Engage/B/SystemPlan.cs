using Engage.C;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.B
{
    public class SystemPlan
    {
        internal static readonly Dictionary<string, string> RealNames = new Dictionary<string, string>()
        {
            {"string","System.String" },
            {"number","System.Int32" },
        };

        public string NS;
        public string TopType;
        private Dictionary<string, TypePlan> Types = new Dictionary<string, TypePlan>();
        private static Dictionary<string, string> TypeAliases = new Dictionary<string, string>();
        private HashSet<string> BoolFlags = new HashSet<string>();
        private HashSet<string> IntFlags = new HashSet<string>();
        private Dictionary<string, List<TokenPlan>> Tokens = new Dictionary<string, List<TokenPlan>>();
        private Dictionary<string, List<HandlerPlan>> Handlers = new Dictionary<string, List<HandlerPlan>>();

        public SystemPlan(string ns)
        {
            NS = ns;
        }

        public static string Dealias(string name)
            => TypeAliases.ContainsKey(name) ? RealNames[TypeAliases[name]] : name;

        public static string Unalias(string name)
            => TypeAliases.ContainsKey(name) ? TypeAliases[name] : name;

        internal void AddBoolFlag(string name)
        {
            if (!String.IsNullOrEmpty(name))
                BoolFlags.Add(name);
        }

        internal void AddIntFlag(string name)
        {
            if (!String.IsNullOrEmpty(name))
                IntFlags.Add(name);
        }

        internal void NormaliseFlags()
        {
            foreach (string f in BoolFlags.Distinct().ToArray())
                if (f.EndsWith("#"))
                {
                    IntFlags.Add(f.Substring(0, f.Length - 1));
                    BoolFlags.Remove(f);
                }
        }

        internal string AllBoolFlags()
            => String.Join(", ", BoolFlags);

        internal string AllIntFlags()
            => String.Join(", ", IntFlags);

        internal void AddHandler(HandlerPlan hp)
        {
            string type = hp.ReactOn.Value;
            foreach (var k in Tokens.Keys)
                if (Tokens[k].Contains(hp.ReactOn))
                    type = k;
            if (String.IsNullOrEmpty(type))
                Console.WriteLine($"[A2B] Cannot determine type of token '{hp.ReactOn.Value}'");
            if (!Handlers.ContainsKey(type))
                Handlers[type] = new List<B.HandlerPlan>();
            Handlers[type].Add(hp);
        }

        internal void AddToken(string type, List<B.TokenPlan> ts)
        {
            Tokens[type] = ts;
        }

        internal TypePlan GetTypePlan(string name)
        {
            if (Types.ContainsKey(name))
                return Types[name];
            if (TypeAliases.ContainsKey(name))
                return GetTypePlan(TypeAliases[name]);
            if (RealNames.ContainsKey(name))
                return new TypePlan(RealNames[name]);
            Console.WriteLine($"[ B ] Failed to get a type plan for '{name}'");
            return null;
        }

        internal bool HasType(string name)
            => Types.ContainsKey(name);

        internal void AddType(string n, string super, bool silent = false)
        {
            if (String.IsNullOrEmpty(n))
                return;
            if (Types.Count == 0)
            {
                TopType = n;
                Console.WriteLine($"[A2B] Top type is assumed to be {TopType}");
            }
            if (Types.ContainsKey(n))
            {
                if (!silent)
                    Console.WriteLine($"[A2B] Cannot add type '{n}' the second time");
                return;
            }
            TypePlan tp = new TypePlan(n);
            tp.Super = super;
            Console.WriteLine($"[A2B] Added type '{n}' to the plan");
            Types[tp.Name] = tp;
        }

        internal void InferTypeAliases()
        {
            foreach (var t in Tokens.Keys)
            {
                if (t == "mark" || t == "skip" || t == "word")
                    continue;
                foreach (B.TokenPlan tok in Tokens[t])
                    if (tok.Special)
                        TypeAliases[t] = tok.Value;
            }
        }

        public IEnumerable<C.CsClass> GenerateDataClasses()
            => Types.Values
                .Where(t => !t.IsList)
                .Select(t => t.GenerateClass(NS));

        public C.CsClass GenerateParser()
        {
            var p = new C.CsClass();
            p.NS = NS;
            p.Name = "Parser";
            p.Super = "BaseParser";
            p.AddUsing("EngageRuntime");
            p.AddUsing("System");
            p.AddUsing("System.Collections.Generic");
            if (BoolFlags.Count > 0)
                p.AddField(String.Join(", ", BoolFlags.OrderBy(x => x)), "bool", isPublic: false);
            if (IntFlags.Count > 0)
                p.AddField(String.Join(", ", IntFlags.OrderBy(x => x)), "int", isPublic: false);
            // token types
            var tt = new C.CsEnum();
            tt.IsPublic = false;
            tt.Name = "TokenType";
            tt.Add("TUndefined");
            tt.Add("TEOF");
            tt.Add(Tokens.Keys.Where(t => t != "skip").Select(t => "T" + t));
            p.AddInner(tt);
            // parser constructor
            var pc = new C.CsConstructor();
            pc.InheritFromBase = true;
            pc.AddArgument("input", "string");
            p.AddConstructor(pc);
            // the parse function
            var pf = new C.CsMethod();
            pf.Name = "Parse";
            pf.RetType = "object";
            pf.AddCode("string ERROR = \"\"");
            pf.AddCode("TokenType type");
            pf.AddCode("string lexeme");
            List<C.CsStmt> loop = new List<C.CsStmt>();
            var pl = new C.WhileStmt("type != TokenType.TEOF", reversed: true);

            // main parsing loop: begin
            pl.AddCode("var _token = NextToken();");
            pl.AddCode("lexeme = _token.Item2;");
            pl.AddCode("type = _token.Item1;");

            var swType = new C.SwitchCaseStmt();
            swType.Expression = "type";

            var UsedTokens = new HashSet<string>();
            UsedTokens.Add("skip");

            foreach (var hpk in Handlers.Keys)
            {
                List<C.CsStmt> branchType = new List<C.CsStmt>();
                if (hpk == "EOF")
                    branchType.Add(new C.SimpleStmt("Flush()"));
                if (Handlers[hpk].Count == 1)
                {
                    Handlers[hpk][0].GenerateAbstractCode(branchType);
                }
                else
                {
                    var swLex = new C.SwitchCaseStmt();
                    // much faster to switch-case on a char than on a string
                    bool matchChar = Handlers[hpk].Select(hp => hp.ReactOn.Value).All(v => v.Length == 1);
                    swLex.Expression = "lexeme" + (matchChar ? "[0]" : "");
                    // Need this dance because there may be different actions for the same token with different guards
                    Dictionary<TokenPlan, Tuple<List<string>, List<List<HandleAction>>>> resortedHandlers = new Dictionary<TokenPlan, Tuple<List<string>, List<List<HandleAction>>>>();
                    foreach (var hp in Handlers[hpk])
                    {
                        if (!resortedHandlers.ContainsKey(hp.ReactOn))
                            resortedHandlers[hp.ReactOn] = new Tuple<List<string>, List<List<HandleAction>>>(new List<string>(), new List<List<HandleAction>>());
                        resortedHandlers[hp.ReactOn].Item1.Add(hp.GuardFlag);
                        hp.AddRecipeTo(resortedHandlers[hp.ReactOn].Item2);
                    }
                    List<TokenPlan> resortedKeys = resortedHandlers.Keys.ToList();
                    resortedKeys.Sort((x, y) => y.Value.Length - x.Value.Length);
                    foreach (var key in resortedKeys)
                        GenerateLexBranch(swLex, hpk, resortedHandlers[key].Item1, resortedHandlers[key].Item2, key, matchChar);
                    branchType.Add(swLex);
                }
                swType.Branches["TokenType.T" + hpk] = branchType;
                UsedTokens.Add(hpk);
            }
            foreach (var t in Tokens.Keys)
            {
                if (!UsedTokens.Contains(t))
                    Console.WriteLine($"[B2C] unused token {t}");
                foreach (B.TokenPlan tok in Tokens[t])
                {
                    if (!tok.Special)
                        continue;
                    List<C.CsStmt> branchType = new List<C.CsStmt>();
                    string todo = "";
                    switch (tok.Value)
                    {
                        case "number":
                            todo = "System.Int32.Parse(lexeme)";
                            break;

                        case "string":
                            todo = "lexeme";
                            break;
                    }
                    todo = PossiblyWrap(todo, tok.Value);
                    branchType.Add(new C.SimpleStmt($"Push({todo})"));

                    swType.Branches["TokenType.T" + t] = branchType;
                }
            }

            pl.AddCode(swType);
            var cond = "!System.String.IsNullOrEmpty(ERROR)";
            var abend = new C.IfThenElse();
            abend.AddToBranch(cond, "Console.WriteLine(\"Parser error: \" + ERROR);");
            abend.AddToBranch(cond, "return null;");
            pl.AddCode(abend);
            // main parsing loop: end

            pf.AddCode(pl);
            pf.AddCode(new C.IfThenElse($"Main.Peek() is {TopType}", "return Main.Pop()"));
            pf.AddCode("return null"); // TODO!!!
            p.AddMethod(pf);

            // other methods
            GenerateTokeniser(p);

            return p;
        }

        private string PossiblyWrap(string v, string key)
        {
            foreach (var t in TypeAliases.Keys)
                if (TypeAliases[t] == key && Handlers.ContainsKey(t))
                {
                    // NB: there can be only one
                    var x = Handlers[t][0].IsPushFirst();
                    if (String.IsNullOrEmpty(x))
                        Console.WriteLine("[B2C] some unsupported functionality found");
                    else
                        return $"new {x}({v})";
                }
            return v;
        }

        private void GenerateLexBranch(SwitchCaseStmt swLex, string hpk, List<string> guardFlags, List<List<HandleAction>> recipes, TokenPlan reactOn, bool matchChar)
        {
            List<C.CsStmt> branchLex = new List<C.CsStmt>();
            //Console.WriteLine($"[IR] in '{hpk}', handle {hp.ReactOn.Value}");
            bool onlyWraps = true;
            var ite = new C.IfThenElse();
            for (int i = 0; i < guardFlags.Count; i++)
            {
                if (!String.IsNullOrEmpty(guardFlags[i]))
                    ite.AddBranch(guardFlags[i]);
                var target = String.IsNullOrEmpty(guardFlags[i]) ? branchLex : ite.ThenBranches[guardFlags[i]];
                foreach (var action in recipes[i])
                {
                    if (!(action is WrapOne))
                        onlyWraps = false;
                    if (action != null)
                    {
                        if (action is WrapOne waction)
                        {
                            List<CsStmt> fake = new List<CsStmt>();
                            action.GenerateAbstractCode(fake);
                            if (fake.Count == 1 && fake[0] is IfThenElse ite2)
                            {
                                var newcond = guardFlags[i] + " && " + ite2.ThenBranches.Keys.First();
                                ite.RenameBranch(guardFlags[i], newcond);
                                foreach (CsStmt stmt in ite2.ThenBranches.Values.First())
                                    ite.AddToBranch(newcond, stmt);
                            }
                            else
                                Console.WriteLine("[ERR] This particular usage of WRAP is not supported yet");
                        }
                        else
                            action.GenerateAbstractCode(target);
                    }
                    else
                        Console.WriteLine($"[B2C] Warning: no action to handle '{hpk}'/{reactOn.Value}");
                }
            }
            string flags;
            if (guardFlags.Count == 1)
                flags = "flag " + guardFlags[0] + " is not";
            else
                flags = "neither of the flags " + String.Join(", ", guardFlags) + " are";
            branchLex.Add(ite);
            if (!onlyWraps)
                if (guardFlags.Any(f => !String.IsNullOrEmpty(f)))
                    ite.AddElse($"ERROR = \"{flags} lifted when expected\"");
            if (matchChar)
                swLex.Branches["'" + reactOn.Value + "'"] = branchLex;
            else
                swLex.Branches['"' + reactOn.Value + '"'] = branchLex;
        }

        private void GenerateTokeniser(C.CsClass cls)
        {
            var skipmark = new List<string>();

            var tok = new C.CsMethod();
            tok.IsPublic = false;
            tok.Name = "NextToken";
            tok.RetType = "Tuple<TokenType, string>";

            // init phase
            tok.AddCode("TokenType t = TokenType.TUndefined;");
            tok.AddCode("string s = \"\";");
            // EOF phase
            tok.AddCode(new C.IfThenElse("pos >= input.Length", "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")"));
            // skip
            if (Tokens.ContainsKey("skip"))
            {
                string cond = String.Join(" || ", Tokens["skip"].Select(t => $"input[pos] == '{t.Value}'"));
                tok.AddCode(new C.WhileStmt($"pos < input.Length && ({cond})", "pos++"));
                Tokens["skip"].ForEach(t => skipmark.Add(t.Value));
            }
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'skip'");
            // EOF after skip
            var MegaIf = new C.IfThenElse("pos >= input.Length", "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")");
            tok.AddCode(MegaIf);
            // mark
            if (Tokens.ContainsKey("mark"))
            {
                Tokens["mark"].ForEach(t => skipmark.Add(t.Value));
                GenerateBranches("mark", MegaIf, null);
            }
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'word'");
            // word
            if (Tokens.ContainsKey("word"))
                GenerateBranches("word", MegaIf, skipmark);
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'word'");
            // number etc
            foreach (var tt in Tokens.Keys)
            {
                if (tt == "skip" || tt == "word" || tt == "mark")
                    continue;
                GenerateBranches(tt, MegaIf, skipmark);
            }
            tok.AddCode("return new Tuple<TokenType, string>(t, s);");

            cls.AddMethod(tok);
        }

        // Precondition: Tokens.Contains(token_name)
        private void GenerateBranches(string token_name, C.IfThenElse ite, List<string> skipmark)
        {
            if (!Tokens.ContainsKey(token_name))
                return;
            foreach (var tm in Tokens[token_name])
            {
                Tuple<string, IEnumerable<CsStmt>> res;
                if (tm.Special)
                    res = GenerateBranchSpecialMatch(tm.Value, token_name);
                else
                    res = GenerateBranchPreciseMatch(tm.Value, token_name, skipmark);
                if (res != null)
                    ite.AddToBranch(res.Item1, res.Item2);
            }
        }

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchSpecialMatch(string value, string type)
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

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchStringMatch(string type)
        {
            string cond = "";
            if (Tokens.ContainsKey("skip"))
                foreach (var t in Tokens["skip"])
                    cond += $" && input[pos] != '{t.Value}'";
            var block = new List<CsStmt>();
            block.Add(new SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new C.WhileStmt($"pos < input.Length{cond}", "s += input[pos++]"));
            return new Tuple<string, IEnumerable<CsStmt>>(null, block); // null condition means the ELSE branch
        }

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchNumberMatch(string type)
        {
            var block = new List<CsStmt>();
            string cond = string.Join(" || ", "0123456789".Select(c => $"input[pos] == '{c}'"));
            block.Add(new SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new C.WhileStmt($"pos < input.Length && ({cond})", "s += input[pos++]"));
            return new Tuple<string, IEnumerable<CsStmt>>(cond, block);
        }

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchPreciseMatch(string value, string type, List<string> skipmark)
        {
            int len = value.Length;
            var block = new List<CsStmt>();
            string cond;
            if (len > 1)
                cond = $"pos + {len - 1} < input.Length";
            else
                cond = "";
            for (int i = 0; i < len; i++)
                cond += $" && input[pos + {i}] == '{value[i]}'";
            if (cond.StartsWith(" && "))
                cond = cond.Substring(4);

            // either EOF or next is skip or mark
            if (skipmark != null && skipmark.Count > 0)
                cond = $"({cond}) && (pos + {len} == input.Length || {String.Join(" || ", skipmark.Select(c => $"input[pos + {len}] == '{c}'"))})";

            cond = cond.Replace(" + 0", "");
            block.Add(new SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new SimpleStmt($"s = \"{value}\""));
            block.Add(new SimpleStmt(len > 1 ? $"pos += {len}" : "pos++"));
            return new Tuple<string, IEnumerable<CsStmt>>(cond, block);
        }
    }
}

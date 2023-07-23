using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.NA
{
    // Dictionary<TokenPlan,HandlerCollection>
    internal class HandlerMetaCollection
    {
        private readonly Dictionary<NA.TokenPlan, HandlerCollection> _data = new();

        public void Add(NA.HandlerPlan hp)
        {
            _data.TryAdd(hp.ReactOn, new HandlerCollection());
            _data[hp.ReactOn].AddGuardAndRecipe(hp);
        }

        public IEnumerable<NA.TokenPlan> SortedKeys()
        {
            var resortedKeys = _data.Keys.ToList();
            resortedKeys.Sort((x, y) => y.Value.Length - x.Value.Length);
            return resortedKeys;
        }

        public IReadOnlyList<string> GuardFlags(NA.TokenPlan tp) => _data[tp].GuardFlags;
        public IReadOnlyList<List<NA.HandleAction>> Recipes(NA.TokenPlan tp) => _data[tp].Recipes;
    }

    // Tuple<List<string>, List<List<HandleAction>>>
    internal class HandlerCollection
    {
        private readonly List<NA.TokenPlan> _reacts = new();
        private readonly List<string> _triggers = new();
        private readonly List<List<NA.HandleAction>> _handlers = new();
        public IReadOnlyList<string> GuardFlags => _triggers;
        public IReadOnlyList<List<NA.HandleAction>> Recipes => _handlers;

        public void AddGuardAndRecipe(NA.HandlerPlan hp)
        {
            for (int i = 0; i < _triggers.Count; i++)
                if (_triggers[i] == hp.GuardFlag && _reacts[i].Value == hp.ReactOn.Value)
                {
                    hp.AddRecipeTo(_handlers[i]);
                    return;
                }

            // if branch not found, make a new one
            _reacts.Add(hp.ReactOn);
            _triggers.Add(hp.GuardFlag);
            hp.AddRecipeTo(_handlers);
        }
    }

    public class SystemPlan
    {
        internal static readonly Dictionary<string, string> RealNames = new()
        {
            { "string", "System.String" },
            { "number", "System.Int32" },
        };

        private readonly string NS;
        private string TopType;
        private readonly Dictionary<string, NA.TypePlan> Types = new();
        private static Dictionary<string, string> TypeAliases = new();
        private readonly HashSet<string> BoolFlags = new();
        private readonly HashSet<string> IntFlags = new();
        private readonly Dictionary<string, List<NA.TokenPlan>> Tokens = new();
        private readonly Dictionary<string, List<NA.HandlerPlan>> Handlers = new();

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

        internal void AddHandler(NA.HandlerPlan hp)
        {
            string type = hp.ReactOn.Value;
            foreach (var k in Tokens.Keys)
                if (Tokens[k].Contains(hp.ReactOn))
                    type = k;
            if (String.IsNullOrEmpty(type))
                Console.WriteLine($"[NC->NA] Cannot determine type of token '{hp.ReactOn.Value}'");
            if (!Handlers.ContainsKey(type))
                Handlers[type] = new List<NA.HandlerPlan>();
            Handlers[type].Add(hp);
        }

        internal void AddToken(string type, List<NA.TokenPlan> ts)
        {
            Tokens[type] = ts;
        }

        internal NA.TypePlan GetTypePlan(string name)
        {
            if (Types.TryGetValue(name, out var typePlan))
                return typePlan;
            if (TypeAliases.TryGetValue(name, out var typeAlias))
                return GetTypePlan(typeAlias);
            if (RealNames.TryGetValue(name, out var realName))
                return new NA.TypePlan(realName);
            Console.WriteLine($"[ NA ] Failed to get a type plan for '{name}'");
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
                Console.WriteLine($"[NC->NA] Top type is assumed to be {TopType}");
            }

            if (Types.ContainsKey(n))
            {
                if (!silent)
                    Console.WriteLine($"[NC->NA] Cannot add type '{n}' the second time");
                return;
            }

            var tp = new NA.TypePlan(n) { Super = super };
            Console.WriteLine($"[NC->NA] Added type '{n}' to the plan");
            Types[tp.Name] = tp;
        }

        internal void InferTypeAliases()
        {
            foreach (var t in Tokens.Keys)
            {
                if (t is "mark" or "skip" or "word")
                    continue;
                foreach (var tok in Tokens[t].Where(tok => tok.Special))
                    TypeAliases[t] = tok.Value;
            }
        }

        public IEnumerable<GA.CsClass> GenerateDataClasses()
            => Types.Values
                .Where(t => !t.IsList)
                .Select(t => t.GenerateClass(NS));

        public GA.CsClass GenerateParser()
        {
            var p = new GA.CsClass
            {
                NS = NS,
                Name = "Parser",
                Super = "BaseParser"
            };
            p.AddUsing("EngageRuntime");
            p.AddUsing("System");
            p.AddUsing("System.Collections.Generic");
            if (BoolFlags.Count > 0)
                p.AddField(String.Join(", ", BoolFlags.OrderBy(x => x)), "bool", isPublic: false);
            if (IntFlags.Count > 0)
                p.AddField(String.Join(", ", IntFlags.OrderBy(x => x)), "int", isPublic: false);
            // token types
            var tt = new GA.CsEnum
            {
                IsPublic = false,
                Name = "TokenType"
            };
            tt.Add("TUndefined");
            tt.Add("TEOF");
            tt.Add(Tokens.Keys.Where(t => t != "skip").Select(t => "T" + t));
            p.AddInner(tt);
            // parser constructor
            var pc = new GA.CsConstructor
            {
                InheritFromBase = true
            };
            pc.AddArgument("input", "string");
            p.AddConstructor(pc);
            // the parse function
            var pf = new GA.CsMethod
            {
                Name = "Parse",
                RetType = "object"
            };
            pf.AddCode("string ERROR = \"\"");
            pf.AddCode("TokenType type");
            pf.AddCode("string lexeme");

            // beginning of file
            if (Handlers.TryGetValue("BOF", out var bofHanfler))
                foreach (var plan in bofHanfler)
                    plan.GenerateAbstractCode(pf);

            // the loop
            var loop = new List<GA.CsStmt>();
            var pl = new GA.WhileStmt("type != TokenType.TEOF", reversed: true);

            // main parsing loop: begin
            pl.AddCode("var _token = NextToken();");
            pl.AddCode("lexeme = _token.Item2;");
            pl.AddCode("type = _token.Item1;");

            var swType = new GA.SwitchCaseStmt { Expression = "type" };

            var usedTokens = new HashSet<string> { "skip" };

            foreach (var hpk in Handlers.Keys)
            {
                var branchType = new List<GA.CsStmt>();
                if (hpk == "BOF")
                    continue;
                if (hpk == "EOF")
                    branchType.Add(new GA.SimpleStmt("Flush()"));
                if (Handlers[hpk].Count == 1)
                {
                    Handlers[hpk][0].GenerateAbstractCode(branchType);
                }
                else
                {
                    var swLex = new GA.SwitchCaseStmt();
                    // much faster to switch-case on a char than on a string
                    bool matchChar = Handlers[hpk].Select(hp => hp.ReactOn.Value).All(v => v.Length == 1);
                    swLex.Expression = "lexeme" + (matchChar ? "[0]" : "");
                    // Need this dance because there may be different actions for the same token with different guards
                    HandlerMetaCollection resortedHandlers = new HandlerMetaCollection();
                    foreach (var hp in Handlers[hpk])
                        resortedHandlers.Add(hp);
                    foreach (var key in resortedHandlers.SortedKeys())
                        GenerateLexBranch(swLex, hpk, resortedHandlers.GuardFlags(key), resortedHandlers.Recipes(key),
                            key, matchChar);
                    branchType.Add(swLex);
                }

                swType.Branches["TokenType.T" + hpk] = branchType;
                usedTokens.Add(hpk);
            }

            foreach (var t in Tokens.Keys)
            {
                if (!usedTokens.Contains(t))
                    Console.WriteLine($"[NA->GA] unused token {t}");
                foreach (NA.TokenPlan tok in Tokens[t])
                {
                    if (!tok.Special)
                        continue;
                    var branchType = new List<GA.CsStmt>();
                    string todo = tok.Value switch
                    {
                        "number" => "System.Int32.Parse(lexeme)",
                        "string" => "lexeme",
                        _ => ""
                    };
                    todo = PossiblyWrap(todo, tok.Value);
                    branchType.Add(new GA.SimpleStmt($"Push({todo})"));

                    swType.Branches["TokenType.T" + t] = branchType;
                }
            }

            pl.AddCode(swType);
            const string cond = "!System.String.IsNullOrEmpty(ERROR)";
            var abend = new GA.IfThenElse();
            abend.AddToBranch(cond, "Console.WriteLine(\"Parser error: \" + ERROR);");
            abend.AddToBranch(cond, "return null;");
            pl.AddCode(abend);
            // main parsing loop: end

            pf.AddCode(pl);
            pf.AddCode(new GA.IfThenElse($"Main.Peek() is {TopType}", "return Main.Pop()"));
            pf.AddCode("return null"); // TODO!!!
            p.AddMethod(pf);

            // other methods
            GenerateTokeniser(p);

            return p;
        }

        private string PossiblyWrap(string v, string key)
        {
            foreach (var x in
                     from t in TypeAliases.Keys
                     where TypeAliases[t] == key && Handlers.ContainsKey(t)
                     select Handlers[t][0].IsPushFirst())
                if (String.IsNullOrEmpty(x))
                    Console.WriteLine("[NA->GA] some unsupported functionality found");
                else
                    return $"new {x}({v})";
            return v;
        }

        private static void GenerateLexBranch(GA.SwitchCaseStmt swLex, string hpk, IReadOnlyList<string> guardFlags,
            IReadOnlyList<List<NA.HandleAction>> recipes, NA.TokenPlan reactOn, bool matchChar)
        {
            bool failOtherwise = true;
            var branchLex = new List<GA.CsStmt>();
            Console.WriteLine($"[IR] in '{hpk}', handle {reactOn.Value}");
            bool onlyWraps = true;
            var ite = new GA.IfThenElse();
            for (int i = 0; i < guardFlags.Count; i++)
            {
                List<GA.CsStmt> target;
                if (String.IsNullOrEmpty(guardFlags[i]))
                    target = branchLex;
                else if (guardFlags[i] == "ANY")
                {
                    failOtherwise = false;
                    target = ite.ElseBranch;
                }
                else
                    target = ite.GetThenBranch(guardFlags[i]);

                foreach (var action in recipes[i])
                {
                    if (!(action is NA.WrapOne))
                        onlyWraps = false;
                    if (action != null)
                    {
                        if (action is NA.WrapOne)
                        {
                            var fake = new List<GA.CsStmt>();
                            action.GenerateAbstractCode(fake);
                            if (fake.Count == 1 && fake[0] is GA.IfThenElse ite2)
                            {
                                var newcond = guardFlags[i] + " && " + ite2.FirstThenBranchKey();
                                ite.RenameBranch(guardFlags[i], newcond);
                                foreach (GA.CsStmt stmt in ite2.FirstThenBranchValue())
                                    ite.AddToBranch(newcond, stmt);
                            }
                            else
                                Console.WriteLine("[ERR] This particular usage of WRAP is not supported yet");
                        }
                        else
                            action.GenerateAbstractCode(target);
                    }
                    else
                        Console.WriteLine($"[NA->GA] Warning: no action to handle '{hpk}'/{reactOn.Value}");
                }
            }

            branchLex.Add(ite);

            if (failOtherwise)
            {
                string flags;
                var realFlags = guardFlags.Where(f => f != "ANY").ToList();
                if (realFlags.Count == 1)
                    flags = "flag " + guardFlags[0] + " is not";
                else
                    flags = "neither of the flags " + String.Join(", ", realFlags) + " are";
                if (!onlyWraps)
                    if (realFlags.All(f => !String.IsNullOrEmpty(f)))
                        ite.AddElse($"ERROR = \"{flags} lifted when expected\"");
            }

            if (matchChar)
                swLex.Branches["'" + reactOn.Value + "'"] = branchLex;
            else
                swLex.Branches['"' + reactOn.Value + '"'] = branchLex;
        }

        private void GenerateTokeniser(GA.CsClass cls)
        {
            var skipmark = new List<string>();

            var tok = new GA.CsMethod
            {
                IsPublic = false,
                Name = "NextToken",
                RetType = "Tuple<TokenType, string>"
            };

            // init phase
            tok.AddCode("TokenType t = TokenType.TUndefined;");
            tok.AddCode("string s = \"\";");
            // EOF phase
            tok.AddCode(new GA.IfThenElse("Pos >= Input.Length",
                "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")"));
            // skip
            if (Tokens.ContainsKey("skip"))
            {
                string cond = String.Join(" || ", Tokens["skip"].Select(t => $"Input[Pos] == '{t.Value}'"));
                tok.AddCode(new GA.WhileStmt($"Pos < Input.Length && ({cond})", "Pos++"));
                Tokens["skip"].ForEach(t => skipmark.Add(t.Value));
            }
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'skip'");

            // EOF after skip
            var megaIf = new GA.IfThenElse("Pos >= Input.Length",
                "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")");
            tok.AddCode(megaIf);
            // mark
            if (Tokens.TryGetValue("mark", out var markTokens))
            {
                markTokens.ForEach(t => skipmark.Add(t.Value));
                GenerateBranches("mark", megaIf, null);
            }
            else
                Console.WriteLine("[IR] It is suspicious that there are no tokens of type 'word'");

            // word
            if (Tokens.ContainsKey("word"))
                GenerateBranches("word", megaIf, skipmark);
            else
                Console.WriteLine("[IR] It is suspicious that there are no tokens of type 'word'");
            // number etc
            foreach (var tt in Tokens.Keys.Where(tt => tt != "skip" && tt != "word" && tt != "mark"))
                GenerateBranches(tt, megaIf, skipmark);
            tok.AddCode("return new Tuple<TokenType, string>(t, s);");

            cls.AddMethod(tok);
        }

        // Precondition: Tokens.Contains(token_name)
        private void GenerateBranches(string tokenName, GA.IfThenElse ite, List<string> skipMark)
        {
            if (!Tokens.ContainsKey(tokenName))
                return;
            Tokens[tokenName]
                .Select(tm => GenerateOneBranch(tm.Special, tm.Value, tokenName, skipMark))
                .Where(res => res != null)
                .ToList()
                .ForEach(res => ite.AddToBranch(res.Item1, res.Item2));
        }

        private Tuple<string, IEnumerable<GA.CsStmt>> GenerateOneBranch(
            bool special,
            string value,
            string tokenName,
            IEnumerable<string> skipMarks)
            => special
                ? GenerateBranchSpecialMatch(value, tokenName)
                : GenerateBranchPreciseMatch(value, tokenName, skipMarks);

        private Tuple<string, IEnumerable<GA.CsStmt>> GenerateBranchSpecialMatch(string value, string type)
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

        private Tuple<string, IEnumerable<GA.CsStmt>> GenerateBranchStringMatch(string type)
        {
            string cond = "";
            if (Tokens.TryGetValue("skip", out var skipTokens))
                foreach (var t in skipTokens)
                    cond += $" && Input[Pos] != '{t.Value}'";
            if (Tokens.TryGetValue("mark", out var markTokens))
                foreach (var t in markTokens)
                    cond += $" && Input[Pos] != '{t.Value}'";
            var block = new List<GA.CsStmt>
            {
                new GA.SimpleStmt($"t = TokenType.T{type}"),
                new GA.WhileStmt($"Pos < Input.Length{cond}", "s += Input[Pos++]")
            };
            return new Tuple<string, IEnumerable<GA.CsStmt>>(null, block); // null condition means the ELSE branch
        }

        private static Tuple<string, IEnumerable<GA.CsStmt>> GenerateBranchNumberMatch(string type)
        {
            var block = new List<GA.CsStmt>();
            string cond = string.Join(" || ", "0123456789".Select(c => $"Input[Pos] == '{c}'"));
            block.Add(new GA.SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new GA.WhileStmt($"Pos < Input.Length && ({cond})", "s += Input[Pos++]"));
            return new Tuple<string, IEnumerable<GA.CsStmt>>(cond, block);
        }

        private static Tuple<string, IEnumerable<GA.CsStmt>> GenerateBranchPreciseMatch(string value, string type,
            IEnumerable<string> skipmark)
        {
            int len = value.Length;
            var block = new List<GA.CsStmt>();
            var cond = len > 1 ? $"Pos + {len - 1} < Input.Length" : "";
            for (int i = 0; i < len; i++)
                cond += $" && Input[Pos + {i}] == '{value[i]}'";
            if (cond.StartsWith(" && "))
                cond = cond[4..];

            // either EOF or next is skip or mark
            if (skipmark != null && skipmark.Any())
                cond =
                    $"({cond}) && (Pos + {len} == Input.Length || {String.Join(" || ", skipmark.Select(c => $"Input[Pos + {len}] == '{c}'"))})";

            cond = cond.Replace(" + 0", "");
            block.Add(new GA.SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new GA.SimpleStmt($"s = \"{value}\""));
            block.Add(new GA.SimpleStmt(len > 1 ? $"Pos += {len}" : "Pos++"));
            return new Tuple<string, IEnumerable<GA.CsStmt>>(cond, block);
        }
    }
}
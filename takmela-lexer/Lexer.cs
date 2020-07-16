using System;
using System.Collections.Generic;

namespace takmelalexer
{
	public class Lexer
	{
		private bool writeDotFiles;
		private Dictionary<string, FA> ruleDfas;
		private string errorRuleName;
		private Dictionary<string, int> vocab;
		private Set<string> skipRules;
		private Dictionary<int, string> classOf; // must be initialized after vocab

		private Stack<string> scopes;
		private Dictionary<string, List<string>> pushers;
		private Dictionary<string, List<string>> poppers;

		private Dictionary<string, List<Tuple<string, FA>>> dfasByScope;

		private string inputText;
		private int pos;

		private int line, col;

		public Lexer(LexerModule m, Dictionary<string, int> tokenVocab, bool _writeDotFiles)
		//throws IOException
		{
			writeDotFiles = _writeDotFiles;
			ruleDfas = initAutomata (m.Rules);
			errorRuleName = m.ErrorRuleName;
			vocab = tokenVocab;
			skipRules = initSkip (m.Rules);
			classOf = initTokenClasses (m.Rules);
			dfasByScope = initScopes (m.Rules, ruleDfas);
		}

		public Lexer(List<LexerRule> rules, Dictionary<string, int> tokenVocab, string _errorRuleName, bool _writeDotFiles)
		//throws IOException
		{
			writeDotFiles = _writeDotFiles;
			ruleDfas = initAutomata(rules);
			errorRuleName = _errorRuleName;
			vocab = tokenVocab;
			skipRules = initSkip (rules);
			classOf = initTokenClasses (rules);
			dfasByScope = initScopes (rules, ruleDfas);
		}

		public Lexer(LexerModule m, bool _writeDotFiles)
		//throws IOException
		{
			writeDotFiles = _writeDotFiles;
			ruleDfas = initAutomata (m.Rules);
			errorRuleName = m.ErrorRuleName;
			vocab = initDefaultVocab (m.Rules, m.ErrorRuleName);
			skipRules = initSkip (m.Rules);
			classOf = initTokenClasses (m.Rules);
			dfasByScope = initScopes (m.Rules, ruleDfas);
		}

		public Lexer(List<LexerRule> rules, string _errorRuleName, bool _writeDotFiles)
		//throws IOException
		{
			writeDotFiles = _writeDotFiles;
			ruleDfas = initAutomata (rules);
			errorRuleName = _errorRuleName;
			vocab = initDefaultVocab (rules, _errorRuleName);
			skipRules = initSkip (rules);
			classOf = initTokenClasses (rules);
			dfasByScope = initScopes (rules, ruleDfas);
		}


		private	static Dictionary<string, int> initDefaultVocab(List<LexerRule> rules, string errorRuleName)
		{
			Dictionary<string, int> vocab = new Dictionary<string, int>();

			int c = 0;
			foreach (LexerRule r in rules)
			{
				vocab[r.Name] = c++;
			}

			if(errorRuleName != "")
			{
				vocab[errorRuleName] = c++;
			}
			return vocab;
		}

		private	Dictionary<string, List<Tuple<string, FA>> > initScopes(List<LexerRule> rules, 
			Dictionary<string, FA> ruleDfas)
		{
			pushers = new Dictionary<string, List<string>> ();
			poppers = new Dictionary<string, List<string>> ();

			Dictionary<string, List<Tuple<string, FA>>> ret = new Dictionary<string, List<Tuple<string, FA>>>();
			foreach(LexerRule r in rules)
			{
				string name = r.Name;
				FA fa = ruleDfas[name];
				foreach(string scope in r.Within)
				{
					Utils.Add(ret, scope, new Tuple<string, FA>(name, fa));
				}
				if(r.Within.Count == 0)
				{
					Utils.Add(ret, "", new Tuple<string, FA>(name, fa));
				}

				pushers[name] = r.Pushes;
				poppers[name] = r.Pops;
			}
			return ret;
		}

		private	Set<string> initSkip(List<LexerRule> rules)
		{
			Set<string> ret = new Set<string>();
			foreach (LexerRule r in rules)
			{
				if (r.Skip)
				{
					ret.Add(r.Name);
				}
			}
			return ret;
		}

		private void validate(List<LexerRule> rules)
		{
			Set<string> ruleNames = new Set<string>();
			foreach(LexerRule r in rules)
			{
				string rn = r.Name;
				if(ruleNames.Contains(rn))
				{
					throw new Exception(string.Format("Duplicate lexer rule! {0}",rn));
				}
				ruleNames.Add(rn);
			}
		}

		private	Dictionary<string, FA> initAutomata(List<LexerRule> rules)
		{
			Dictionary<string, FA> ruleNfas = new Dictionary<string, FA>();
			FAAlgo algo = new FAAlgo();

			validate(rules);

			if(writeDotFiles)
			{
				Utils.EnsureDirExists("./dots");
			}

			foreach (LexerRule r in rules)
			{
				ruleNfas[r.Name] = algo.nfaFromRegEx(r.Expr);
			}

			if(writeDotFiles)
			{
				foreach (KeyValuePair<string, FA> kv in ruleNfas)
				{
					string dot = TestLexer.ToDot(kv.Value);
					Utils.WriteToFile("./dots/" + kv.Key + ".dot", dot);
				}
			}
			//*/

			Dictionary<string, FA> ruleDfas = new Dictionary<string, FA>();

			foreach (KeyValuePair<string, FA> kv in ruleNfas)
			{
				string name = kv.Key;
				FA dfa = algo.dfaFromNfa(kv.Value);
				ruleDfas[name] = dfa;
				if(writeDotFiles)
				{
					string dot = TestLexer.ToDot(dfa);
					Utils.WriteToFile("./dots/" + name + "_dfa.dot", dot);
				}
			}

			Dictionary<string, FA> ruleNoCommon = new Dictionary<string, FA> ();

			foreach (KeyValuePair<string, FA> kv in ruleDfas)
			{
				FA noCommonTrans = algo.nfaWithNoCommonTransitions(kv.Value);
				ruleNoCommon[kv.Key] = noCommonTrans;

				if(writeDotFiles)
				{
					string dot = TestLexer.ToDot(noCommonTrans);
					Utils.WriteToFile("./dots/" + kv.Key + "_noCommon.dot", dot);
				}
			}

			foreach (KeyValuePair<string, FA> kv in ruleNoCommon)
			{
				string name = kv.Key;
				FA dfa = algo.dfaFromNfa(kv.Value);
				ruleDfas[name] = dfa;
				if(writeDotFiles)
				{
					string dot = TestLexer.ToDot(dfa);
					Utils.WriteToFile("./dots/" + name + "_dfa2.dot", dot);
				}
			}
			return ruleDfas;
		}

		private	Dictionary<int, string> initTokenClasses(List<LexerRule> rules)
		{
			Dictionary<int, string> ret = new Dictionary<int, string>();
			foreach(LexerRule r in rules)
			{
				if(r.Class != "")
				{
					int tokenType = vocab[r.Name];
					ret[tokenType] = r.Class;
				}
			}

			if(errorRuleName != "")
			{
				// An error rule is unique, and is always its own class
				// TODO vocab[errorRuleName] should throw if key doesn't exist
				int tokenType = vocab[errorRuleName];
				ret[tokenType] = errorRuleName;
			}
			return ret;
		}

		public Dictionary<string, int> getTokenVocab() { return vocab; }

		public void init(string input)
		{
			inputText = input;
			pos = 0;
			line = col = 0;
			scopes = new Stack<string>();
		}

		public bool hasMoreTokens()
		{
			return pos < inputText.Length;
		}

		// Longest match wins
		// in case of tie, first-in-rule-listing wins
		public Token nextToken() // throws LexerError
		{
			string maxAcceptingRule = "";
			int maxAcceptingPos = 0, maxAcLine = -1, maxAcCol = -1;

			string scope = "";
			if(scopes.Count !=0)
			{
				scope = scopes.Peek();
			}

			foreach (Tuple<string, FA> kv in dfasByScope[scope])
			{
				string ruleName = kv.Item1;
				FA fa = kv.Item2;
				//log("Rule %s, pos=%s", ruleName, pos);
				int state = fa.startState;

				int p = pos;
				int __line = line;
				int __col = col;
				while (true)
				{
					if (p == inputText.Length)
					{
						if (fa.acceptingStates.Contains(state))
						{
							return accept(ruleName, p, __line, __col);
						}
						else
						{
							string ex = "No recognizable token at end of file";
							throw new Exception(ex);
						}
					}

					char c = inputText[p];

					bool found = false;
					foreach (Tuple<Trans, int > t in fa.outTrans(state))
					{
						//Utils.printf("....Match %s?", t.a);
						if (t.Item1.match(c))
						{
							state = t.Item2;
							found = true;
							break;
						}
						//Utils.printf("....fails");
					}
					if (found)
					{
						p++;
						__col++;
						if (c == '\n')
						{
							__line++;
							__col = 0;
						}
					}
					else
					{
						if (fa.acceptingStates.Contains(state))
						{
							if (p > maxAcceptingPos)
							{
								maxAcceptingPos = p;
								maxAcceptingRule = ruleName;
								maxAcLine = __line;
								maxAcCol = __col;
								goto forRules;
							}
							else
							{
								goto forRules;
							}
						}
						else
						{
							goto forRules;
						}
					}
				} // while
				forRules:;
			} // for kv in rules
			if (maxAcceptingPos > pos)
			{
				return accept(maxAcceptingRule, maxAcceptingPos, maxAcLine, maxAcCol);
			}
			else
			{
				if(errorRuleName != "")
				{
					int advLine, advCol;
					advance(pos, line, col, out advLine, out advCol);
					Token t = accept(errorRuleName, pos+1, advLine, advCol);
					return t;
				}
				string ex = "Cannot process input near: " + formatLiteral(Utils.Mid(inputText, pos, 15)) + "";
				throw new Exception(ex);
			}
		}

		public int tokenId(string ruleName)
		{
			return vocab[ruleName];
		}

		public string classOfToken(int type) { return classOf[type]; }

		private string formatLiteral(string s)
		{
			return s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
		}

		private void advance(int pos, int line, int col, out int newLine, out int newCol)
		{
			char c = inputText[pos];
			newLine = line;
			newCol = col + 1;
			if(c == '\n')
			{
				newCol = 0;
				newLine++;
			}
		}

		private List<string> emptyList = new List<string>();

		private Token accept(string ruleName, int p, int line, int col)
		{
			string lexeme = inputText.Substring(pos, p-pos);
			Token t = new Token(tokenId(ruleName), lexeme, line, col, pos, skipRules.Contains(ruleName));
			pos = p;
			this.line = line;
			this.col = col;
			
			foreach(string psh in Utils.GetOrDefault(pushers, ruleName, emptyList))
			{
				scopes.Push(psh);
			}

			foreach(string pp in Utils.GetOrDefault(poppers, ruleName, emptyList))
			{
				string s = scopes.Peek();
				if(s!= pp) { throw new Exception("Unable to pop scope '" + (pp) +"', found scope '" + s +"'"); }
				scopes.Pop();
			}

			return t;
		}
	}
}


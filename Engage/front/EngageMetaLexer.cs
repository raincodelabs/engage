using System;
using System.Collections.Generic;
using takmelalexer;

namespace Engage.front
{
	public class EngageMetaLexer
	{
		private Dictionary<string, int> tokenVocab = new Dictionary<string, int>();
		int tokVocabCount;

		public Dictionary<string, int> TokenVocab { get { return tokenVocab; } }

		public List<Token> Lexise(string engageGrammar)
		{
			List<LexerRule> rules = new List<LexerRule> ();

			kw (rules, "pop");
			kw (rules, "pop*");
			kw (rules, "pop#");
			kw (rules, "await");
			kw (rules, "await*");
			kw (rules, "tear");

			kw (rules, "push");
			kw (rules, "wrap");
			kw (rules, "lift");
			kw (rules, "drop");
			kw (rules, "trim");

			kw (rules, "number");
			kw (rules, "string");
			kw (rules, "EOF");

			kw (rules, "namespace");
			kw (rules, "types");
			kw (rules, "tokens");
			kw (rules, "handlers");

			kw (rules, "where");
			kw (rules, "upon");
			kw (rules, "with");

			s (rules, "comma", ",");
			s (rules, "semi", ";");
			s (rules, "is_type", "::");
			s (rules, "sub_type", "<:");
			s (rules, "star", "*");
			s (rules, "assign", ":=");
			s (rules, "arrow", "->");
			s (rules, "lparen", "(");
			s (rules, "rparen", ")");

			r(rules, "id", 
				seq (
					cc(ch('A', 'Z'), ch('a', 'z'), ch('_')),
					star(cc(ch('A', 'Z'), ch('a', 'z'), ch('0', '9'), ch('#'), ch('_')))
				));
			
			r(rules, "quoted", 
				seq(
					cc(ch('\'')),
					star(nt(ch('\''))),
					cc(ch('\''))
				)
			);

			r_skip(rules, "ws", 
				plus(
					oring(
						cc(ch(' ')),
						cc(ch('\r')),
						cc(ch('\n')),
						cc(ch('\t'))
					)));

			r_skip(rules, "comment", 
				seq(cc(ch('%')),
					star(
						nt(ch('\r'),
						   ch('\n')
						))));

			LexerModule m = new LexerModule (rules, "");

			Lexer lex = new Lexer (m, tokenVocab, true);
			List<Token> result = new List<Token> ();

			lex.init (engageGrammar);
			while (lex.hasMoreTokens ()) 
			{
				Token t = lex.nextToken ();
				if (!t.skip) 
				{
					result.Add (t);
				}
			}
			//Console.WriteLine ("Tokens = {0}", Utils.Join(result, ", "));
			return result;
		}

		private void r(List<LexerRule> rules, string name, RExpr ex)
		{
			LexerRule rule = new LexerRule (name, new List<string> (), new List<string> (), new List<string> (),
				"", false, ex);
			rules.Add (rule);
			tokenVocab [name] = tokVocabCount++;
		}

		private void s(List<LexerRule> rules, string name, String lexeme)
		{
			LexerRule rule = new LexerRule (name, new List<string> (), new List<string> (), new List<string> (),
				"", false, new Str(lexeme));
			rules.Add (rule);
			tokenVocab [name] = tokVocabCount++;
		}

		private void kw(List<LexerRule> rules, String lexeme)
		{
			string name = lexeme.Replace ("*", "_star_").Replace ("#", "_hash_");
			LexerRule rule = new LexerRule (name, new List<string> (), new List<string> (), new List<string> (),
				"", false, new Str(lexeme));
			rules.Add (rule);
			tokenVocab [name] = tokVocabCount++;
		}

		private void r_skip(List<LexerRule> rules, string name, RExpr ex)
		{
			LexerRule rule = new LexerRule (name, new List<string> (), new List<string> (), new List<string> (),
				"", true, ex);
			rules.Add (rule);
			tokenVocab [name] = tokVocabCount++;
		}

		private static RXSeq seq(params RExpr[] exprs)
		{
			return new RXSeq(Utils.list(exprs));
		}

		private static Oring oring(params RExpr[] exprs)
		{
			return new Oring(Utils.list(exprs));
		}

		private static Star star(RExpr expr)
		{
			return new Star(expr);
		}

		private static Plus plus(RExpr expr)
		{
			return new Plus(expr);
		}

		private static CharClass cc(params CharClassPart[] pts)
		{
			return new CharClass(Utils.list(pts));
		}

		private static NotCharClass nt(params CharClassPart[] pts)
		{
			return new NotCharClass(Utils.list(pts));
		}

		private static CharClassPart ch(char a, char b)
		{
			return new CharPartRange (a, b);
		}

		private static CharClassPart ch(char a)
		{
			return new CharPartSingle(a);
		}

	}
}


using System;
using System.Collections.Generic;

namespace takmelalexer
{
	public interface CharClassPart
	{
	}

	public class CharPartRange : CharClassPart
	{
		public char From,  To;

		public CharPartRange(char _From, char _To)
		{
			From = _From;
			To = _To;
		}

		public override string ToString()
		{
			return string.Format("CharRange({0}, {1})", From, To);
		}
	}


	public class CharPartSingle : CharClassPart
	{
		public char Ch;

		public CharPartSingle(char _Ch)
		{
			Ch = _Ch;
		}

		public override string ToString()
		{
			return string.Format("CharSingle({0})", Ch);
		}
	}

	public interface RExpr
	{
		
	}


	public class ByName : RExpr
	{
		public string Name;

		ByName(string _Name)
		{
			Name = _Name;
		}

		public override string ToString()
		{
			return string.Format("ByName({0})", Name);
		}
	}

	public class CharClass : RExpr
	{
		public List<CharClassPart> Parts;

		public CharClass(List<CharClassPart> _Parts)
		{
			Parts = _Parts;
		}

		CharClass(params CharClassPart[] _Parts)
		{
			Parts = new List<CharClassPart> ();
			Parts.AddRange(_Parts);
		}

		public override string ToString()
		{
			return string.Format("CharClass({0})",Utils.SurroundJoin(Parts, "[", "]", ", "));
		}
	}

	public class NotCharClass : RExpr
	{
		public List<CharClassPart> Parts;

		public NotCharClass(List<CharClassPart> _Parts)
		{
			Parts = _Parts;
		}

		public override string ToString()
		{
			return string.Format("NotCharClass({0})", Utils.SurroundJoin(Parts, "[", "]", ", "));
		}
	}

	public class Oring : RExpr
	{
		public List<RExpr> Exprs;

		public Oring(List<RExpr> _Exprs)
		{
			Exprs = _Exprs;
		}

		public override string ToString()
		{
			return string.Format("Oring({0})", Utils.SurroundJoin(Exprs, "[", "]", ", "));
		}
	}

	public class RXSeq : RExpr
	{
		public List<RExpr> Exprs;

		public RXSeq(List<RExpr> _Exprs)
		{
			Exprs = _Exprs;
		}

		public override string ToString()
		{
			return string.Format("RXSeq({0})", Utils.SurroundJoin(Exprs, "[", "]", ", "));
		}
	}

	public class Plus : RExpr
	{
		public RExpr Expr;

		public Plus(RExpr _Expr)
		{
			Expr = _Expr;
		}

		public override string ToString()
		{
			return string.Format("Plus({0})", Expr);
		}
	}

	public class Question : RExpr
	{
		public RExpr Expr;

		public Question(RExpr _Expr)
		{
			Expr = _Expr;
		}

		public override string ToString()
		{
			return string.Format("Question({0})", Expr);
		}
	}

	public class Star : RExpr
	{
		public RExpr Expr;

		public Star(RExpr _Expr)
		{
			Expr = _Expr;
		}

		public override string ToString()
		{
			return string.Format("Star({0})", Expr);
		}
	}

	public class Str : RExpr
	{
		public string Value;

		public Str(string _value)
		{
			Value = _value;
		}

		public override string ToString()
		{
			return string.Format("Str({0})", Value);
		}
	}

	public class LexerRule
	{
		public string Name;
		public List<string> Within = new List<string>();
		public List<string> Pushes = new List<string>();
		public List<string> Pops = new List<string>();
		public string Class = "";
		public bool Skip = false;
		public RExpr Expr;

		public LexerRule(string _Name,
			List<string> _Within,
			List<string> _Pushes,
			List<string> _Pops,
			string _Class,
			bool _Skip,
			RExpr _Expr)
		{
			Name = _Name;
			Within = _Within;
			Pushes = _Pushes;
			Pops = _Pops;
			Class = _Class;
			Skip = _Skip;
			Expr = _Expr;
		}

		LexerRule(string _Name, RExpr _Expr)
		{
			Name = _Name;
			Expr = _Expr;
		}

		LexerRule(string _Name, RExpr _Expr, bool _Skip)
		{
			Name = _Name;
			Expr = _Expr;
			Skip = _Skip;
		}

		public override string ToString()
		{
			return string.Format("LexerRule({0}, {1}, {2}, {3}, {4}, {5}, {6})",
				Name,
				Utils.SurroundJoin(Within, "[", "]", ", "),
				Utils.SurroundJoin(Pushes, "[", "]", ", "),
				Utils.SurroundJoin(Pops, "[", "]", ", "),
				Class,
				Skip,
				Expr);
		}
	}

	public class LexerModule
	{
		public List<LexerRule> Rules;
		public string ErrorRuleName;
		public LexerModule(List<LexerRule> _Rules, string _ErrorRuleName)
		{
			Rules = _Rules;
			ErrorRuleName = _ErrorRuleName;
		}

		public override string ToString()
		{
			return string.Format("Module({0})", Utils.SurroundJoin(Rules, "[", "]", ", "));
		}
	}
}


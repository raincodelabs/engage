using System;

namespace takmelalexer
{
	public class Token
	{
		public int id;
		public string text;
		public int line, col, pos;
		public bool skip;

		public Token(int __id, string __text, int __line, int __col, int __pos, bool __skip)
		{
			id = __id;
			text = __text;
			line = __line;
			col = __col;
			pos = __pos;
			skip = __skip;
		}

		public Token() {}

		public override string ToString()
		{
			return text;
		}
	}
}


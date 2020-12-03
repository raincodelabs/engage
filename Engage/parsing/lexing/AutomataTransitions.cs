using System;

namespace takmelalexer
{
	public interface Trans
	{
		bool match (char c);
	}

	public class CharRange : Trans
	{
		public char From, To;

		public CharRange(char _From, char _To)
		{
			From = _From;
			To = _To;
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1}]", From, To);
		}

		public bool match(char c)
		{
			return From <= c && c <= To;
		}

		public override bool Equals(object o)
		{
			CharRange c = o as CharRange;
			if (c == null) { return false; }

			return From == c.From && To == c.To;
		}

		public override int GetHashCode ()
		{
			int prime = 31;
			int result = 1;
			result = prime * result + From;
			result = prime * result + To;
			return result;
		}
	}

	public class Epsilon : Trans
	{
		public bool match(char c)
		{
			throw new Exception("Epsilon shouldn't be used to match chars");
		}

		public override bool Equals(object o)
		{
			return o is Epsilon;
		}

		public override int GetHashCode ()
		{
			return 31;
		}
	}
}


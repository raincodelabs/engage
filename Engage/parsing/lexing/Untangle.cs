using System;
using System.Collections.Generic;

namespace takmelalexer
{
	public class Untangle
	{
		public List<CharRange> onlyA = new List<CharRange>();
		public CharRange common;
		public List<CharRange> onlyB = new List<CharRange>();

		public Untangle() {}

		public Untangle(List<CharRange> _onlyA, CharRange _common, List<CharRange> _onlyB)
		{
			onlyA = _onlyA;
			common = _common;
			onlyB = _onlyB;
		}

		public override string ToString()
		{
			return string.Format("(common={0}, onlyA={1}, onlyB={2})",
				common,
				Utils.Join(onlyA, ", "),
				Utils.Join(onlyB, ", "));
		}

		public static Untangle untangle(int aFrom, int aTo, int bFrom, int bTo)
		{
			if(aFrom > bTo || aFrom > aTo)
			{
				Untangle result = new Untangle();
				result.onlyA.Add(new CharRange((char) aFrom, (char) aTo));
				result.onlyB.Add(new CharRange((char) bFrom, (char) bTo));
				result.common = null;
				return result;
			}
			else
			{
				bool swap = false;

				if(aFrom > bFrom)
				{
					swap = true;

					int temp = aFrom;
					aFrom = bFrom;
					bFrom = temp;

					temp = aTo;
					aTo = bTo;
					bTo = temp;
				}

				if(aFrom == bFrom && aTo > bTo)
				{
					swap = true;

					int temp = aFrom;
					aFrom = bFrom;
					bFrom = temp;

					temp = aTo;
					aTo = bTo;
					bTo = temp;
				}

				Untangle ret = untangle__(aFrom, aTo, bFrom, bTo);
				if(swap)
				{
					List<CharRange> temp = ret.onlyA;
					ret.onlyA = ret.onlyB;
					ret.onlyB = temp;
				}
				return ret;
			}
		}
		/*
		Input: - Two INCLUSIVE ranges
		       - ...that must have a part in common
		       - ...and must be canonicalized as follows:
		            * a.from <= b.from                  is always true
		            * a.to < b.to if a.from==b.from     is always true
		            * from <= to                        is always true for a & b
		Output: Three disjoint sets: (A - common, common, B - common)
		        each set can be zero, one or more char ranges
		This code is very tricky, needs a lot of testing
		*/
		private static Untangle untangle__(int aFrom, int aTo, int bFrom, int bTo)
		{
			Untangle ret = new Untangle();

			int f1 = aFrom;
			int t1 = bFrom -1;
			if(f1 <= t1) { ret.onlyA.Add(new CharRange((char) f1, (char) t1)); }

			f1 = bTo + 1;
			t1 = aTo;
			if(f1 <= t1) { ret.onlyA.Add(new CharRange((char) f1, (char) t1)); }

			f1 = aTo +1;
			t1 = bTo;
			if(f1 <= t1) { ret.onlyB.Add(new CharRange((char) f1, (char) t1)); }

			f1 = bFrom;
			t1 = Math.Min(aTo, bTo);
			ret.common = new CharRange((char) f1, (char) t1);

			return ret;
		}
	}
}


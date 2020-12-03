using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace takmelalexer
{
	public static class TestLexer
	{
		public static String ToDot(FA fa)
		{
			Set<int> allStates = Utils.make_set(Utils.transitiveClosure(fa.startState,
				(s) => Utils.make_set(Utils.GetOrDefault(fa.tg, s, new List<Tuple<Trans, int>>()).Select(p => p.Item2))));

			StringBuilder sb = new StringBuilder();
			p(sb, "digraph NFA {");
			p(sb, "rankdir=LR;");

			foreach (int s in allStates)
			{
				if (fa.acceptingStates.Contains(s))
				{
					p(sb, string.Format("s{0}[fontsize=11, label=\"{1}\", shape=doublecircle, fixedsize=true, width=.6];", 
						s,
						s));
				}
				else
				{
					p(sb, String.Format(
						"s{0}[fontsize=11, label=\"{1}\", shape=circle, fixedsize=true, width=.55, peripheries=1];",
						s,
						s));
				}
			}

			foreach (KeyValuePair<int, List<Tuple<Trans, int>>> kv in fa.tg)
			{
				int from = kv.Key;
				foreach (Tuple<Trans, int> pair in kv.Value)
				{
					Trans t = pair.Item1;
					int to = pair.Item2;
					string label = null;
					if (t is Epsilon)
					{
						label = "&epsilon;";
					}
					else if (t is CharRange)
					{
						CharRange r = (CharRange) t;
						label = string.Format("{{{0}..{1}}}", r.From, r.To);
					}
					else
					{
						throw new System.ArgumentException (t.ToString ());
					}
					p(sb, String.Format(
						"s{0} -> s{1} [fontsize=11, fontname=\"Courier\", arrowsize=.7, label = \"{2}\", arrowhead = normal];",
						from, to, label));
				}
			}

			p(sb, "}");
			return sb.ToString();
		}

		private static void p(StringBuilder sb, String str)
		{
			sb.Append(str + "\n");
		}

	}
}


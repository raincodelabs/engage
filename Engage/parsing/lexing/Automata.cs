using System;
using System.Collections.Generic;
using System.Linq;

namespace takmelalexer
{
	public class FA
	{
		public Dictionary<int, List<Tuple<Trans, int>>> tg = new Dictionary<int, List<Tuple<Trans, int>>>();
		public int startState;
		public Set<int> acceptingStates;

		public FA() {}

		public FA(int startState, int acceptingState)
		{
			this.startState = startState;
			this.acceptingStates = Utils.make_set1(acceptingState);
		}

		public FA(int startState, Set<int> acceptingStates)
		{
			this.startState = startState;
			this.acceptingStates = Utils.make_set<int>(acceptingStates);
		}

		public FA(int startState, int acceptingState, Dictionary<int , List<Tuple<Trans, int>>> tg)
		{
			this.startState = startState;
			this.acceptingStates = Utils.make_set1(acceptingState);
			this.tg = tg;
		}

		public FA(int startState, Set<int> acceptingStates, Dictionary<int , List<Tuple<Trans, int>>> tg)
		{
			this.startState = startState;
			this.acceptingStates = Utils.make_set<int>(acceptingStates);
			this.tg = tg;
		}

		public FA trans(int from, int to, Trans t)
		{
			Utils.Add(tg, from, new Tuple<Trans,int>(t, to));
			return this;
		}

		public FA trans(int from, int to, IEnumerable<Trans> ts)
		{
			foreach(var t in ts)
			{
				Utils.Add(tg, from, new Tuple<Trans,int>(t, to));
			}
			return this;
		}

		public FA merge(FA fa)
		{
			foreach(KeyValuePair<int, List<Tuple<Trans, int>>> p in fa.tg)
			{
				tg.Add(p.Key, p.Value);
			}
			return this;
		}

		public FA merge(IEnumerable<FA> fas)
		{
			foreach(var fa in fas)
			{
				merge (fa);
			}
			return this;
		}

		// Transition out of a given state
		public List<Tuple<Trans, int>> outTrans(int state)
		{
			if (tg.ContainsKey (state)) 
			{
				return tg [state];
			}
			return new List<Tuple<Trans, int>> ();
		}
	}

	class FAAlgo
	{
		private int stateCounter = 0;

		public FA nfaFromRegEx (RExpr _e)
		{
			if (_e is ByName)
			{
				//ByName e = (ByName) _e;
				throw new System.NotImplementedException ("nfaFromRegEx/ByName not implemented");
			}
			else if (_e is CharClass)
			{
				CharClass e = _e as CharClass;
				e = canonicalized(e);
				int start = newState();
				int end = newState();

				return fa(start, end).trans(start, end, charRange(e));
			}
			else if (_e is NotCharClass)
			{
				NotCharClass __e = _e as NotCharClass;
				CharClass e = canonicalized(__e);
				int start = newState();
				int end = newState();

				return fa(start, end).trans(start, end, charRange(e));
			}
			else if (_e is Oring)
			{
				Oring e = _e as Oring;
				List<FA> opts = Utils.list(e.Exprs.Select(a=> { return nfaFromRegEx(a); }));
				List<int> startEpsilons = Utils.list(opts.Select(a => { return a.startState;}));
				List<int> endEpsilons = Utils.list(opts.Select(a => { return onlyAcceptingState(a); }));

				int start = newState();
				int end = newState();

				FA _fa = fa(start, end).merge(opts);

				for (int i = 0; i < opts.Count; ++i)
				{
					_fa.trans(start, startEpsilons[i], epsilon());
					_fa.trans(endEpsilons[i], end, epsilon());
				}
				return _fa;
			}
			else if (_e is Plus)
			{
				Plus e = _e as Plus;
				FA fa = nfaFromRegEx(e.Expr);
				fa.trans(onlyAcceptingState(fa), fa.startState, epsilon());
				return fa;
			}
			else if (_e is RXSeq)
			{
				RXSeq e = _e as RXSeq;
				if (e.Exprs.Count == 1)
				{
					return nfaFromRegEx(e.Exprs[0]);
				}
				List<FA> opts = Utils.list(e.Exprs.Select(a => { return nfaFromRegEx(a); }));
				FA _fa = fa(opts[0].startState, onlyAcceptingState(Utils.last(opts))).merge(opts);
				for(int i = 0; i < opts.Count - 1; ++i)
				{
					int prev = onlyAcceptingState (opts [i]);
					int next = opts [i + 1].startState;
					_fa.trans(prev, next, epsilon());
				}
				return _fa;
			}
			else if (_e is Star)
			{
				Star e = _e as Star;
				FA fa = nfaFromRegEx(e.Expr);
				fa.trans(onlyAcceptingState(fa), fa.startState, epsilon());
				fa.trans(fa.startState, onlyAcceptingState(fa), epsilon());
				return fa;
			}
			else if (_e is Question)
			{
				Question e = _e as Question;
				FA fa = nfaFromRegEx(e.Expr);
				fa.trans(fa.startState, onlyAcceptingState(fa), epsilon());
				return fa;
			}
			else if (_e is Str)
			{
				Str e = _e as Str;
				List<RExpr> seq = new List<RExpr>();
				// todo: iterating over a string, this would
				// not work with surrogate pairs...etc
				for (int i = 0; i < e.Value.Length; ++i)
				{
					char c = e.Value[i];
					List<CharClassPart> v = Utils.list1((CharClassPart) new CharPartRange (c, c));
					CharClass cc = new CharClass(v);
					seq.Add(cc);
				}
				return nfaFromRegEx(new RXSeq(seq));
			}
			else
			{
				throw new Exception("OptionNotHandledException(_e)");
			}
		}

		public FA nfaWithNoCommonTransitions(FA fa)
		{
			int ss = fa.startState;
			Set<int> _as = fa.acceptingStates;

			Dictionary<int, List<Tuple<Trans, int>>> newTg = new Dictionary<int, List<Tuple<Trans, int>>>();
			foreach(KeyValuePair<int, List<Tuple<Trans,int>>> kv in fa.tg)
			{
				int fromState = kv.Key;
				List<Tuple<Trans, int>> transitions = kv.Value;
				List<Tuple<Trans, int>> newTransitions = removeCommonTransitions(transitions);
				newTg[fromState] = newTransitions;
			}
			return new FA(ss, _as, newTg);
		}

		public FA dfaFromNfa(FA nfa)
		{
			Dictionary<Set<int>, Set<Tuple<Trans, Set<int>>>> newTg = 
				new Dictionary<Set<int>, Set<Tuple<Trans, Set<int>>>>();
			Set<int> startState = epsilonClosure(nfa.startState, nfa);
			Set<Set<int>> newAcceptingStates = new Set<Set<int>>();

			Stack<Set<int>> workList = new Stack<Set<int>>();
			Set<Set<int>> done = new Set<Set<int>>();

			workList.Push(startState);

			while (workList.Count != 0)
			{
				Set<int> state = workList.Peek();
				workList.Pop();
				done.Add(state);
				List<Tuple<Trans, int>> allOutTrans = new List<Tuple<Trans, int>>();

				if (nfa.acceptingStates.intersect(state).Count != 0)
				{
					newAcceptingStates.Add(state);
				}

				foreach(int s in state)
				{
					allOutTrans.AddRange(nfa.outTrans(s));
				}

				Dictionary<Trans, List<Tuple<Trans, int>>> groups = new Dictionary<Trans, List<Tuple<Trans, int>>> ();
				var _groups = allOutTrans.GroupBy(p =>{ return p.Item1; });
				foreach(IGrouping<Trans, Tuple<Trans,int>> g in _groups)
				{
					groups.Add (g.Key, g.ToList());
				}
					

				foreach(KeyValuePair<Trans, List<Tuple<Trans, int>>> kv in groups)
				{
					// kv -> Tuple<shared_ptr<Trans>, vector<Tuple<shared_ptr<Trans>, int>>>
					Trans cond = kv.Key;
					if (!(cond is Epsilon))
					{
						List<int> _states = Utils.list(kv.Value.Select(a=>a.Item2));
						Set<int> states = Utils.make_set(_states);
						states = epsilonClosure(states, nfa);
						Utils.Add(newTg, state, new Tuple<Trans, Set<int>>(cond, states));

						if (!done.Contains(states))
						{
							//Utils.printf("found new state: %s", states);
							workList.Push(states);
						}
					}
				}
			}

			LabellerInt<Set<int>> c = new LabellerInt<Set<int>>();

			FA dfa = new FA(c.labelFor(startState), -1);

			foreach(KeyValuePair<Set<int>, Set<Tuple<Trans, Set<int>>>> kv in newTg)
			{
				int s = c.labelFor(kv.Key);
				Set<Tuple<Trans, int>> _trans2 =
					Utils.make_set(kv.Value.Select(p => { return new Tuple<Trans, int>(p.Item1, c.labelFor(p.Item2));} ));
				List<Tuple<Trans, int>> trans2 = Utils.list(_trans2);
				dfa.tg[s] = trans2;
			}

			Set<int> accepting =  Utils.make_set(newAcceptingStates.Select(s => { return c.labelFor(s); }));
			dfa.acceptingStates = accepting;
			return dfa;
		}

		private Set<int> epsilonClosure(Set<int> state, FA fa)
		{
			Set<int> ret = new Set<int>();
			foreach(int s in state)
			{
				ret.AddRange(epsilonClosure(s, fa));
			}
			return ret;
		}

		private Set<int> epsilonClosure(int state, FA fa)
		{
			return Utils.transitiveClosure(state, s => { return epsilons(s, fa); });
		}

		private Set<int> epsilons(int state, FA fa)
		{
			Set<int> eps = new Set<int>();
			foreach(Tuple<Trans, int> t in fa.outTrans(state))
			{
				if(t.Item1 is Epsilon)
				{
					eps.Add(t.Item2);
				}
			}
			return eps;
		}

		private bool hasCommon(Trans a, Trans b)
		{
			if (a is CharRange && b is CharRange)
			{
				CharRange  aa = a as CharRange;
				CharRange bb = b as CharRange;

				if (bb.From > aa.To || aa.From > bb.To)
				{
					return false;
				}
				return true;
				/*
      			// The following is equivalent to the above (from DeMorgan's laws)
				// even though the above is much more obvious. Math is cool like that.

				// To show it, here are diagrams:
				// Case #1
				//     |a---------------------|
				//              |b------------------|

				// Case #2
				//                |a---------------------|
				//     |b------------------|

				// Case #3
				//                |a----|
				//     |b------------------|

				// Case #4
				//                |a---------------------|
				//                     |b-----------|


				if(bb.From <= aa.To && aa.From <= bb.To)
				{
					return true;
				}
				return false;
				*/
			}
			return false;
		}

		private CharClass canonicalized(CharClass e)
		{
			List<CharClassPart> l3 = new List<CharClassPart>();
			l3.AddRange(canonicalized(e.Parts));
			return new CharClass(l3);
		}

		private bool valid(int a, int b)
		{
			return a<=b;
		}

		private CharClass canonicalized(NotCharClass e)
		{
			/*
   			 Here we convert something like [^a-zA-Z] into a vector of CharPartRange objects
     		 we start with the 'any char' range, from 0 to 0xffff, and we subtract each CharClassPart in turn
    		*/

			// todo:
			// Since the code tries to simplify things by converting
			// all CharClassPart objects into ranges, we will treat 'Any'
			// as a range from 0 to 0xffff (the maximum value for a char, i.e 16-bit Unicode)
			// this probably wreaks havoc with internationalization, but we're using chars
			// and ignoring anything more than 16bit for now anyway. If we later use a more advanced library
			// we need to deal with 'Any' and 'Not' for CharClasses in a more general
			// (and encoding independent) way.

			// Note that the code below assumes the 'excluded' vector is sorted
			// which is provided by the called other canonicalized(..) function
			List<CharPartRange> excluded = canonicalized(e.Parts);

			List<CharClassPart> result = new List<CharClassPart>();
			int start = 0, end = 0xffff;
			for(int i=0; i<excluded.Count; ++i)
			{
				CharPartRange r = excluded[i];
				// It is important for a, b to be SIGNED ints, so that From.unicode()-1 can be negative
				// and To.unicode()+1 doesn't wrap around
				int a = start, b = r.From-1;
				if(valid(a, b))
				{
					result.Add(new CharPartRange((char) a, (char) b));
				}
				start = r.To + 1;
			}

			int aa = start, bb = end;

			if(valid(aa, bb))
			{
				result.Add(new CharPartRange((char) aa, (char) bb));
			}

			return new CharClass(result);
		}

		private List<CharPartRange> canonicalized(List<CharClassPart> e)
		{
			List<CharPartRange> l2 = new List<CharPartRange>();
			foreach(CharClassPart c in e)
			{
				if (c is CharPartSingle)
				{
					CharPartSingle cs = c as CharPartSingle;
					l2.Add(new CharPartRange(cs.Ch, cs.Ch));
				}
				else if (c is CharPartRange)
				{
					CharPartRange cr = c as CharPartRange;
					if (cr.From > cr.To)
					{
						cr = new CharPartRange(cr.To, cr.From);
					}
					l2.Add(cr);
				}
				else
				{
					throw new ArgumentException(c.ToString());
				}
			}

			l2.Sort ((a, b) => a.From.CompareTo(b.From));

			return l2;
		}

		private List<Trans> charRange(CharClass cc)
		{
			List<Trans> ret = new List<Trans>();
			foreach(CharClassPart _cr in cc.Parts)
			{
				// Assumes the CharClass has been canonicalized
				// i.e all parts are ranges
				CharPartRange cr = _cr as CharPartRange;
				ret.Add(new CharRange(cr.From, cr.To));
			}
			return ret;
		}

		private int onlyAcceptingState(FA a)
		{
			if (a.acceptingStates.Count != 1)
			{
				throw new ArgumentException("NFAs should be prepared with exactly one accepting state");
			}
			return a.acceptingStates.First;
		}

		private List<Tuple<Trans, int>> removeCommonTransitions(List<Tuple<Trans, int>> _transitions)
		{
			List<Tuple<Trans, int>> transitions = new List<Tuple<Trans, int>> ();
			transitions.AddRange(_transitions);
			while (true)
			{
				bool toSplit = false;
				Tuple<Trans, int> split1 = null, split2 = null;

				for (int i = 0; i < transitions.Count; ++i)
				{
					Tuple<Trans, int> a = transitions[i];
					for (int j = i + 1; j < transitions.Count; ++j)
					{
						Tuple<Trans, int> b = transitions[j];
						if (a.Item2 != b.Item2 && (!equal(a.Item1, b.Item1)) && hasCommon(a.Item1, b.Item1))
						{
							toSplit = true;
							split1 = a;
							split2 = b;
							goto outer;
						}
					}
				}
				outer:

				if (toSplit)
				{
					Untangle unt = _untangle(split1, split2);
					// todo: does erase handle this specialized item type?
					transitions.Remove(split1);
					// todo: same
					transitions.Remove(split2);

					addTranss(transitions, unt.onlyA, split1.Item2);
					addTranss(transitions, unt.onlyB, split2.Item2);

					transitions.Add(new Tuple<Trans,int>(unt.common, split1.Item2));
					transitions.Add(new Tuple<Trans,int>(unt.common, split2.Item2));
				}
				else
				{
					break;
				}
			}
			return transitions;
		}

		private bool equal(Trans a, Trans b)
		{
			return a.Equals (b);
		}

		private void addTranss(List<Tuple<Trans, int>> transitions, List<CharRange> ranges, int toState)
		{
			foreach(CharRange r in ranges)
			{
				transitions.Add(new Tuple<Trans,int>(r, toState));
			}
		}

		private Untangle _untangle(Tuple<Trans, int> t1, Tuple<Trans, int> t2)
		{
			CharRange a = (CharRange) t1.Item1;
			CharRange b = (CharRange) t2.Item1;

			Untangle u = Untangle.untangle(a.From, a.To, b.From, b.To);
			return u;
		}

		private Trans epsilon()
		{
			return new Epsilon();
		}

		private int newState()
		{
			return stateCounter++;
		}

		private FA fa(int start, int accept)
		{
			return new FA(start, accept);
		}
	}
}


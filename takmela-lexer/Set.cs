using System;
using System.Collections.Generic;

namespace takmelalexer
{
	public class Set<T> : IEnumerable<T>
	{
		private HashSet<T> values = new HashSet<T>();

		public Set ()
		{
		}

		public void Add(T value)
		{
			values.Add (value);
		}

		public void AddRange(IEnumerable<T> values)
		{
			foreach (T v in values) 
			{
				Add (v);
			}
		}

		public bool Contains(T val)
		{
			return values.Contains (val);
		}

		public int Count { get { return values.Count; } }

		public T First 
		{ 
			get 
			{ 
				foreach (T x in values) 
				{
					return x;
				}
				throw new Exception ("Cannot use .First with an empty set");
			}
		}

		public Set<T> intersect(Set<T> other)
		{
			// TODO use the smaller of the two sets in the loop
			Set<T> result = new Set<T> ();
			foreach(T val in this)
			{
				if (other.Contains (val)) 
				{
					result.Add (val);
				}
			}
			return result;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return values.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return values.GetEnumerator ();
		}

		public override bool Equals (object obj)
		{
			return obj is Set<T> && values.SetEquals ((obj as Set<T>).values);
		}

		public override int GetHashCode ()
		{
			SortedSet<T> s = new SortedSet<T> ();
			s.UnionWith (this);

			int prime = 31;
			int result = 1;
			foreach (T val in s) 
			{
				result = prime * result + ((val == null) ? 0 : val.GetHashCode());
			}
			return result;
		}
	}
}


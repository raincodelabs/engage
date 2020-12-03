using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace takmelalexer
{
	public static class Utils
	{
		public static string Join<T>(List<T> list, string sep)
		{
			StringBuilder sb = new StringBuilder ();
			string _sep = "";
			foreach (T value in list) 
			{
				sb.Append (_sep);
				_sep = sep;
				sb.Append (value);
			}
			return sb.ToString ();
		}

		public static string SurroundJoin<T>(IEnumerable<T> list, string left, string right, string sep)
		{
			StringBuilder sb = new StringBuilder ();
			string _sep = "";
			foreach (T value in list) 
			{
				sb.Append (_sep);
				_sep = sep;
				sb.Append (left);
				sb.Append (value);
				sb.Append (right);
			}
			return sb.ToString ();
		}

		public static Set<T> make_set<T>(IEnumerable<T> list)
		{
			Set<T> result = new Set<T> ();
			result.AddRange (list);
			return result;
		}

		public static Set<T> make_set1<T>(T val)
		{
			Set<T> result = new Set<T> ();
			result.Add (val);
			return result;
		}

		public static List<T> list<T>(IEnumerable<T> lst)
		{
			List<T> result = new List<T> ();
			result.AddRange (lst);
			return result;
		}

		public static List<T> list1<T>(T val)
		{
			List<T> result = new List<T> ();
			result.Add(val);
			return result;
		}

		public static T last<T>(List<T> lst)
		{
			return lst [lst.Count - 1];
		}

		public static void Add<Tk, Tv>(Dictionary<Tk, List<Tv>> dict, Tk key, Tv val)
		{
			if (!dict.ContainsKey (key)) 
			{
				dict [key] = new List<Tv> ();
			}
			dict [key].Add (val);
		}

		public static void Add<Tk, Tv>(Dictionary<Tk, Set<Tv>> dict, Tk key, Tv val)
		{
			if (!dict.ContainsKey (key)) 
			{
				dict [key] = new Set<Tv> ();
			}
			dict [key].Add (val);
		}

		public static Tv GetOrDefault<Tk, Tv>(Dictionary<Tk,Tv> dict, Tk key, Tv defVal)
		{
			Tv val;
			if (dict.TryGetValue (key, out val)) 
			{
				return val;
			}
			return defVal;
		}

		public static Set<T> transitiveClosure<T>(T root, Func<T,Set<T>> related)
		{
			return transitiveClosure(root, related, (a, b) => { return a.Contains(b); });
		}

		public static Set<T> transitiveClosure<T>(
			T root,
			Func<T, IEnumerable<T>> related,
			Func<Set<T>, T, bool> containsChecker)
		{
			Set<T> ret = new Set<T>();

			Stack<T> stack = new Stack<T>();
			stack.Push(root);
			while(stack.Count != 0)
			{
				T t = stack.Pop();

				if(ret.Contains(t))
				{
					continue; // we've already processed it
				}
				ret.Add(t);

				IEnumerable<T> trelated = related(t);
				foreach(T t2 in trelated)
				{
					if(!ret.Contains(t2))
					{
						stack.Push(t2);
					}
				}
			}
			return ret;
		}

		public static void EnsureDirExists(string path)
		{
			// TODO check if path is an existing non-directory
			Directory.CreateDirectory (path);
		}

		public static void WriteToFile(string path, string content)
		{
			TextWriter w = new StreamWriter (new FileStream (path, FileMode.Create));
			w.Write (content);
			w.Close ();
		}

		public static string ReadFile(string path)
		{
			TextReader r = new StreamReader (new FileStream (path, FileMode.Open));
			string content = r.ReadToEnd ();
			r.Close ();
			return content;
		}

		// Like string.Substring but forgiving
		public static string Mid(string s, int pos, int n)
		{
			n = Math.Min (n, s.Length - pos);
			return s.Substring (pos, n);
		}

		// Assumes one-to-one
		public static Dictionary<B, A> inverse<A,B>(Dictionary<A, B> dict)
		{
			Dictionary<B,A> result = new Dictionary<B, A> ();
			foreach (KeyValuePair<A,B> kv in dict) 
			{
				result[kv.Value ] = kv.Key;
			}
			return result;
		}
	}
}


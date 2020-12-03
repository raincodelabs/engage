using System;
using System.Collections.Generic;

namespace takmelalexer
{
	public class LabellerInt<T>
	{
		private Dictionary<T, int> _labels = new Dictionary<T, int> ();
		private int _count = 0;

		public int labelFor(T value)
		{
			int r;
			if (!_labels.ContainsKey (value)) 
			{
				r = _count++;
				_labels [value] = r;
			}
			else
			{
				r = _labels [value];
			}

			return r;
		}
	}
}


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TupleList<T1, T2> : List<Tuple<T1, T2>> where T1 : IComparable
{
	public void Add(T1 item, T2 item2)
	{
		Add(new Tuple<T1, T2>(item, item2));
	}

	public void Remove(T1 item, T2 item2)
	{
		Remove(Find((Tuple<T1, T2> a) =>
		{
			return a.Item1.Equals(item) && a.Item2.Equals(item2);
		}));
	}

	public new void Sort()
	{
		Comparison<Tuple<T1, T2>> c = (a, b) => a.Item1.CompareTo(b.Item1);
		base.Sort(c);
	}
}

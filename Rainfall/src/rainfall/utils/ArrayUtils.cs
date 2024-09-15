using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public static class ArrayUtils
{
	public static Span<T> AsSpan<T>(this List<T> list) where T : unmanaged
	{
		return CollectionsMarshal.AsSpan(list);
	}

	public static T[] Copy<T>(T[] arr)
	{
		T[] newArr = new T[arr.Length];
		Array.Copy(arr, newArr, arr.Length);
		return newArr;
	}

	public static T[] Add<T>(T[] arr, T t)
	{
		T[] newArr = new T[arr.Length + 1];
		Array.Copy(arr, newArr, arr.Length);
		newArr[arr.Length] = t;
		return newArr;
	}

	public static T[] RemoveAt<T>(T[] arr, int idx)
	{
		T[] newArr = new T[arr.Length - 1];
		Array.Copy(arr, newArr, idx);
		Array.Copy(arr, idx + 1, newArr, idx, arr.Length - idx - 1);
		return newArr;
	}

	public static T[] Slice<T>(T[] arr, int index, int count = -1)
	{
		if (count == -1)
			count = arr.Length - index;
		T[] newArr = new T[count];
		Array.Copy(arr, index, newArr, 0, count);
		return newArr;
	}
}

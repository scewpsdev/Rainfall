using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Hash
{
	public static uint hash(uint i)
	{
		i = (uint)(i ^ (uint)(61)) ^ (uint)(i >> 16);
		i *= (uint)(9);
		i = i ^ (i >> 4);
		i *= (uint)(0x27d4eb2d);
		i = i ^ (i >> 15);
		return i;
	}

	public static uint combine(uint a, uint b)
	{
		uint u = a;
		u = u * 19 + b;
		return u;
	}

	public static uint hash(int i)
	{
		return hash((uint)i);
	}

	public static uint hash(float f)
	{
		unsafe
		{
			return hash(*(uint*)&f);
		}
	}

	public static uint hash(Vector2i v)
	{
		uint u = hash(v.x);
		u = u * 19 + hash(v.y);
		return u;
	}

	public static uint hash(Vector3i v)
	{
		uint u = hash(v.x);
		u = u * 19 + hash(v.y);
		u = u * 19 + hash(v.z);
		return u;
	}

	public static uint hash(Vector3 v)
	{
		uint u = hash(v.x);
		u = u * 19 + hash(v.y);
		u = u * 19 + hash(v.z);
		return u;
	}

	public static uint hash(Quaternion q)
	{
		uint u = hash(q.x);
		u = u * 19 + hash(q.y);
		u = u * 19 + hash(q.z);
		u = u * 19 + hash(q.w);
		return u;
	}

	// https://stackoverflow.com/questions/2624192/good-hash-function-for-strings
	public static uint hash(string str)
	{
		uint hash = 7;
		for (int i = 0; i < str.Length; i++)
		{
			byte c = (byte)str[i];
			hash = hash * 31 + c;
		}
		return hash;
	}

	public static uint hash(Span<byte> str)
	{
		uint hash = 7;
		int i = 0;
		while (true)
		{
			byte c = str[i++];
			if (c == 0)
				break;
			hash = hash * 31 + c;
		}
		return hash;
	}
}

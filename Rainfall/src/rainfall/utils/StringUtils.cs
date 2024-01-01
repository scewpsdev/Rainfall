using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


public static class StringUtils
{
	public static unsafe int StringLength(byte* str)
	{
		int len = 0;
		while (str[len] != 0)
			len++;
		return len;
	}

	public static int StringLength(Span<byte> str)
	{
		int len = 0;
		while (str[len] != 0)
			len++;
		return len;
	}

	public static int WriteCharacter(Span<byte> dst, int length, char c)
	{
		dst[length++] = (byte)c;
		dst[length] = 0;
		return length;
	}

	public static int WriteCharacter(Span<byte> dst, char c)
	{
		return WriteCharacter(dst, 0, c);
	}

	public static int AppendCharacter(Span<byte> dst, char c)
	{
		return WriteCharacter(dst, StringLength(dst), c);
	}

	public static int WriteString(Span<byte> dst, int length, string str)
	{
		for (int i = 0; i < str.Length; i++)
		{
			dst[length++] = (byte)str[i];
		}
		dst[length] = 0;
		return length;
	}

	public static int WriteString(Span<byte> dst, string str)
	{
		return WriteString(dst, 0, str);
	}

	public static int AppendString(Span<byte> dst, string str)
	{
		return WriteString(dst, StringLength(dst), str);
	}

	public static int WriteDigit(Span<byte> dst, int length, int digit)
	{
		dst[length++] = (byte)('0' + digit);
		dst[length] = 0;
		return length;
	}

	public static int WriteDigit(Span<byte> dst, int digit)
	{
		return WriteDigit(dst, 0, digit);
	}

	public static int AppendDigit(Span<byte> dst, int digit)
	{
		return WriteDigit(dst, StringLength(dst), digit);
	}

	public static int WriteInteger(Span<byte> dst, int length, int number, int digits = 0)
	{
		if (number == 0)
		{
			dst[length++] = (byte)'0';
			dst[length] = 0;
			return length;
		}
		if (number < 0)
		{
			dst[length++] = (byte)'-';
			number = -number;
		}

		Span<byte> buffer = stackalloc byte[50];
		int i = 0;
		while (number != 0 || i < digits)
		{
			buffer[i++] = (byte)('0' + number % 10);
			number = number / 10;
		}

		for (int j = i - 1; j >= 0; j--)
		{
			dst[length++] = buffer[j];
		}

		dst[length] = 0;
		return length;
	}

	public static int WriteInteger(Span<byte> dst, int number)
	{
		return WriteInteger(dst, 0, number);
	}

	public static int AppendInteger(Span<byte> dst, int number)
	{
		return WriteInteger(dst, StringLength(dst), number);
	}

	public static int WriteFloat(Span<byte> dst, int length, float f, int decimalPoints = 6)
	{
		int fi = (int)f;
		if (fi == 0 && f < 0.0f)
			length = WriteCharacter(dst, length, '-');
		length = WriteInteger(dst, length, fi);
		length = WriteCharacter(dst, length, '.');

		float fpart = MathF.Abs(f - fi);
		int fparti = (int)(fpart * MathHelper.IPow(10, decimalPoints));
		length = WriteInteger(dst, length, fparti, decimalPoints);

		return length;
	}

	public static int WriteFloat(Span<byte> dst, float f, int decimalPoints = 6)
	{
		return WriteFloat(dst, 0, f, decimalPoints);
	}

	public static int AppendFloat(Span<byte> dst, float f, int decimalPoints = 6)
	{
		return WriteFloat(dst, StringLength(dst), f, decimalPoints);
	}

	public static int WriteBool(Span<byte> dst, int length, bool b)
	{
		if (b)
			return WriteString(dst, length, "true");
		else
			return WriteString(dst, length, "false");
	}

	public static int WriteBool(Span<byte> dst, bool b)
	{
		return WriteBool(dst, 0, b);
	}

	public static int AppendBool(Span<byte> dst, bool b)
	{
		return WriteBool(dst, StringLength(dst), b);
	}

	public static int WriteStringDigit(Span<byte> dst, string str, int digit)
	{
		int len = WriteString(dst, 0, str);
		return WriteDigit(dst, len, digit);
	}

	public static unsafe bool CompareStrings(string a, byte* b)
	{
		int bLen = StringLength(b);
		if (a.Length != bLen)
			return false;
		for (int i = 0; i < bLen; i++)
		{
			if (a[i] != b[i])
				return false;
		}
		return true;
	}

	public static bool StartsWith(string a, string prefix)
	{
		if (prefix.Length > a.Length)
			return false;
		for (int i = 0; i < prefix.Length; i++)
		{
			if (a[i] != prefix[i])
				return false;
		}
		return true;
	}

	public static bool EndsWith(string a, string suffix)
	{
		if (suffix.Length > a.Length)
			return false;
		for (int i = 0; i < suffix.Length; i++)
		{
			if (a[a.Length - suffix.Length + i] != suffix[i])
				return false;
		}
		return true;
	}
}

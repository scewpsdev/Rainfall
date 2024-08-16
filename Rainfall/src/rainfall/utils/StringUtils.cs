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

	public static int WriteString(Span<byte> dst, int length, Span<byte> str, int strLen)
	{
		for (int i = 0; i < strLen; i++)
		{
			dst[length++] = str[i];
		}
		dst[length] = 0;
		return length;
	}

	public static unsafe int WriteString(byte* dst, int length, byte* str, int strLen)
	{
		for (int i = 0; i < strLen; i++)
		{
			dst[length++] = str[i];
		}
		dst[length] = 0;
		return length;
	}

	public static int WriteString(Span<byte> dst, Span<byte> str, int strLen)
	{
		return WriteString(dst, 0, str, strLen);
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

	public unsafe static int WriteString(Span<byte> dst, int length, byte* str, int strLen)
	{
		for (int i = 0; i < strLen; i++)
		{
			dst[length++] = str[i];
		}
		dst[length] = 0;
		return length;
	}

	public static unsafe int WriteString(byte* dst, int length, string str)
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

	public static unsafe int WriteString(Span<byte> dst, byte* str, int length)
	{
		return WriteString(dst, 0, str, length);
	}

	public static unsafe int WriteString(byte* dst, string str)
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

	public static int WriteInteger(Span<byte> dst, int length, long number, int digits = 0)
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

	public static int WriteInteger(Span<byte> dst, long number)
	{
		return WriteInteger(dst, 0, number);
	}

	public static int AppendInteger(Span<byte> dst, long number)
	{
		return WriteInteger(dst, StringLength(dst), number);
	}

	public static int WriteFloat(Span<byte> dst, int length, float f, int decimalPoints = 6)
	{
		long fi = (long)f;
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

	public static string GetFilenameFromPath(string path)
	{
		int slash = path.LastIndexOfAny(new char[] { '/', '\\' });
		if (slash != -1)
			return path.Substring(slash + 1);
		return path;
	}

	public static void MemoryString(Span<byte> str, long mem)
	{
		if (mem >= 1 << 30)
		{
			AppendFloat(str, mem / (float)(1 << 30), 2);
			AppendString(str, " GB");
		}
		else if (mem >= 1 << 20)
		{
			AppendFloat(str, mem / (float)(1 << 20), 2);
			AppendString(str, " MB");
		}
		else if (mem >= 1 << 10)
		{
			AppendFloat(str, mem / (float)(1 << 10), 2);
			AppendString(str, " KB");
		}
		else
		{
			AppendInteger(str, mem);
			AppendString(str, " B");
		}
	}

	public static string ToRoman(int number)
	{
		if (number >= 1000) return "M" + ToRoman(number - 1000);
		if (number >= 900) return "CM" + ToRoman(number - 900);
		if (number >= 500) return "D" + ToRoman(number - 500);
		if (number >= 400) return "CD" + ToRoman(number - 400);
		if (number >= 100) return "C" + ToRoman(number - 100);
		if (number >= 90) return "XC" + ToRoman(number - 90);
		if (number >= 50) return "L" + ToRoman(number - 50);
		if (number >= 40) return "XL" + ToRoman(number - 40);
		if (number >= 10) return "X" + ToRoman(number - 10);
		if (number >= 9) return "IX" + ToRoman(number - 9);
		if (number >= 5) return "V" + ToRoman(number - 5);
		if (number >= 4) return "IV" + ToRoman(number - 4);
		if (number >= 1) return "I" + ToRoman(number - 1);
		return "";
	}
}

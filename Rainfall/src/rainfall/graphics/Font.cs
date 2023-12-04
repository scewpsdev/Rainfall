using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class FontData
	{
		internal IntPtr handle;


		internal FontData(IntPtr handle)
		{
			this.handle = handle;
		}

		public Font createFont(float size, bool antialiased)
		{
			IntPtr fontHandle = Native.Resource.Resource_CreateFontFromData(handle, size, (byte)(antialiased ? 1 : 0));
			return new Font(fontHandle, this, size);
		}
	}

	public class Font
	{
		internal IntPtr handle;
		internal FontData data;

		public float size { get; private set; }


		internal Font(IntPtr handle, FontData data, float size)
		{
			this.handle = handle;
			this.data = data;
			this.size = size;
		}

		public int measureText(Span<byte> text, int length)
		{
			unsafe
			{
				fixed (byte* textPtr = text)
					return Native.Resource.Resource_FontMeasureText(handle, textPtr, length);
			}
		}

		public int measureText(Span<byte> text)
		{
			return measureText(text, text.Length);
		}

		public int measureText(string text, int length)
		{
			return Native.Resource.Resource_FontMeasureText(handle, text, length);
		}

		public int measureText(string text)
		{
			return measureText(text, text.Length);
		}
	}
}

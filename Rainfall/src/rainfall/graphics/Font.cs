using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Rainfall
{
	public class FontData
	{
		internal IntPtr resource;
		IntPtr handle;


		internal unsafe FontData(IntPtr resource)
		{
			this.resource = resource;
			byte* data = Resource.Resource_MiscGetData(resource, out int size);
			handle = FontData_Create(data, size);
		}

		public Font createFont(float size, bool antialiased)
		{
			IntPtr fontHandle = Resource.Resource_CreateFontFromData(handle, size, (byte)(antialiased ? 1 : 0));
			return new Font(fontHandle, this, size, antialiased);
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr FontData_Create(byte* data, int size);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void FontData_Destroy(IntPtr fontData);
	}

	public class Font
	{
		internal IntPtr handle;
		internal FontData data;

		public float size { get; private set; }
		public bool antialiased { get; private set; }


		internal Font(IntPtr handle, FontData data, float size, bool antialiased)
		{
			this.handle = handle;
			this.data = data;
			this.size = size;
			this.antialiased = antialiased;
		}

		public int measureText(Span<byte> text, int offset, int length)
		{
			unsafe
			{
				fixed (byte* textPtr = text)
					return Resource.Resource_FontMeasureText(handle, textPtr, offset, length);
			}
		}

		public int measureText(Span<byte> text, int length)
		{
			return measureText(text, 0, length);
		}

		public int measureText(Span<byte> text)
		{
			return measureText(text, 0, text.Length);
		}

		public int measureText(string text, int offset, int length)
		{
			return Resource.Resource_FontMeasureText(handle, text, offset, length);
		}

		public int measureText(string text, int length)
		{
			return measureText(text, 0, length);
		}

		public int measureText(string text)
		{
			return measureText(text, 0, text.Length);
		}
	}
}

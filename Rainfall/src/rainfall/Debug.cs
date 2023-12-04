using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public static class Debug
	{
		static bool debugStatsOverlay = false;


		public static void Assert(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}

		public static void DrawDebugText(int x, int y, byte color, Span<byte> text)
		{
			unsafe
			{
				fixed (byte* textPtr = text)
					Native.Graphics.Graphics_DrawDebugText(x, y, color, textPtr);
			}
		}

		public static void DrawDebugText(int x, int y, Span<byte> text)
		{
			unsafe
			{
				fixed (byte* textPtr = text)
					Native.Graphics.Graphics_DrawDebugText(x, y, 0xF, textPtr);
			}
		}

		public static void DrawDebugText(int x, int y, byte color, string text)
		{
			Native.Graphics.Graphics_DrawDebugText(x, y, color, text);
		}

		public static void DrawDebugText(int x, int y, string text)
		{
			Native.Graphics.Graphics_DrawDebugText(x, y, 0xF, text);
		}

		public static bool debugStatsOverlayEnabled
		{
			get { return debugStatsOverlay; }
			set
			{
				Native.Application.Application_SetDebugStatsOverlayEnabled(value);
				debugStatsOverlay = value;
			}
		}

		public static Vector2i debugTextSize
		{
			get
			{
				Native.Graphics.Graphics_GetDebugTextSize(out int width, out int height);
				return new Vector2i(width, height);
			}
		}
	}
}

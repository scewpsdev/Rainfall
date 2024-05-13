using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public static class Debug
	{
		public static void Assert(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}

		public static void CaptureFrame()
		{
			Native.Application.Application_CaptureFrame();
		}

		public static bool debugTextEnabled
		{
			get => Native.Application.Application_IsDebugTextEnabled() != 0;
			set { Native.Application.Application_SetDebugTextEnabled((byte)(value ? 1 : 0)); }
		}

		public static bool debugStatsEnabled
		{
			get => Native.Application.Application_IsDebugStatsEnabled() != 0;
			set { Native.Application.Application_SetDebugStatsEnabled((byte)(value ? 1 : 0)); }
		}

		public static bool debugWireframeEnabled
		{
			get => Native.Application.Application_IsDebugWireframeEnabled() != 0;
			set { Native.Application.Application_SetDebugWireframeEnabled((byte)(value ? 1 : 0)); }
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

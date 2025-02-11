using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public static class Time
	{
		public static bool paused
		{
			set { Native.Application.Application_SetTimerPaused((byte)(value ? 1 : 0)); }
		}

		public static long currentTime => Native.Application.Application_GetCurrentTime();

		public static float gameTime => Native.Application.Application_GetCurrentTime() / 1e9f;

		public static long timestamp => Native.Application.Application_GetTimestamp();

		public static float deltaTime => Native.Application.Application_GetFrameTime() / 1e9f;

		public static int fps => Native.Application.Application_GetFPS();

		public static float ms => Native.Application.Application_GetMS();

		public static long nativeMemory => Native.Application.Application_GetMemoryUsage();

		public static int numAllocations => Native.Application.Application_GetNumAllocations();

		public static void GetTopAllocators(int num, Span<byte> files, Span<long> sizes)
		{
			unsafe
			{
				fixed (byte* filesPtr = files)
				fixed (long* sizesPtr = sizes)
					Native.Application.Application_GetTopAllocators(num, filesPtr, sizesPtr);
			}
		}
	}
}

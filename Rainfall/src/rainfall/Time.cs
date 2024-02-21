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
		public static long currentTime
		{
			get { return Native.Application.Application_GetCurrentTime(); }
		}

		public static long timestamp
		{
			get { return Native.Application.Application_GetTimestamp(); }
		}

		public static float deltaTime
		{
			get { return Native.Application.Application_GetFrameTime() / 1e9f; }
		}

		public static float physicsDelta
		{
			get { return Native.Physics.Physics_GetSimulationDelta() / 1e9f; }
		}

		public static int fps
		{
			get { return Native.Application.Application_GetFPS(); }
		}

		public static float ms
		{
			get { return Native.Application.Application_GetMS(); }
		}

		public static long memory
		{
			get { return GC.GetTotalMemory(false); }
		}

		public static long nativeMemory
		{
			get { return Native.Application.Application_GetMemoryUsage(); }
		}

		public static int numAllocations
		{
			get { return Native.Application.Application_GetNumAllocations(); }
		}

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

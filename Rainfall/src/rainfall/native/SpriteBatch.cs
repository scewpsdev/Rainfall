using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Rainfall.Native
{
	internal static class SpriteBatch
	{
		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr SpriteBatch_Create();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SpriteBatch_Destroy(IntPtr handle);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SpriteBatch_Begin(IntPtr batch, int numDrawCommands);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SpriteBatch_End(IntPtr batch);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int SpriteBatch_GetNumDrawCalls(IntPtr batch);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SpriteBatch_SubmitDrawCall(IntPtr batch, int idx, int pass, IntPtr shader);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SpriteBatch_Draw(IntPtr batch,
			float x0, float y0, float z0, float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3,
			float nx0, float ny0, float nz0, float nx1, float ny1, float nz1, float nx2, float ny2, float nz2, float nx3, float ny3, float nz3,
			float u0, float v0, float u1, float v1, float u2, float v2, float u3, float v3,
			float r, float g, float b, float a, float mask,
			ushort texture, uint textureFlags);
	}
}

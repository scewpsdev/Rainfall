using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class LineRenderer
	{
		IntPtr handle;


		public LineRenderer()
		{
			handle = LineRenderer_Create();
		}

		public void destroy()
		{
			LineRenderer_Destroy(handle);
		}

		public void begin(int numDrawCommands)
		{
			LineRenderer_Begin(handle, numDrawCommands);
		}

		public void end(int pass, Shader shader, GraphicsDevice graphics)
		{
			graphics.setPrimitiveType(PrimitiveType.Lines);
			LineRenderer_End(handle, pass, shader.handle);
		}

		public void draw(Vector3 vertex0, Vector3 vertex1, Vector4 color)
		{
			LineRenderer_Draw(handle, vertex0, vertex1, color);
		}


		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr LineRenderer_Create();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void LineRenderer_Destroy(IntPtr handle);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void LineRenderer_Begin(IntPtr handle, int numDrawCommands);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void LineRenderer_End(IntPtr handle, int pass, IntPtr shader);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		static extern void LineRenderer_Draw(IntPtr handle, Vector3 vertex0, Vector3 vertex1, Vector4 color);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Rainfall.Native
{
	internal static class ImGui
	{
		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ImGui_ShowDemoWindow();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ImGui_ShowUserGuide();
	}
}

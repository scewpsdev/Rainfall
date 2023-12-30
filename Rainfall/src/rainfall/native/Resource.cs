using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall.Native
{
	internal static class Resource
	{
		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_CreateShader(byte* vertexPath, byte* fragmentPath);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_CreateShaderCompute(byte* computePath);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_ReadImageFromFile(byte* path, out TextureInfo info);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_ImageGetData(IntPtr image);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Resource_FreeImage(IntPtr image);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe ushort Resource_CreateTexture2DFromFile(byte* path, ulong flags, out TextureInfo info);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe ushort Resource_CreateCubemapFromFile(byte* path, ulong flags, out TextureInfo info);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_CreateSceneDataFromFile(byte* path, ulong textureFlags);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_CreateModelFromSceneData(IntPtr scene);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_ModelGetSceneData(IntPtr model);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_CreateFontDataFromFile(byte* path);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Resource_CreateFontFromData(IntPtr data, float size, byte antialiased);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Resource_FontMeasureText(IntPtr font, byte* text, int length);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int Resource_FontMeasureText(IntPtr font, [MarshalAs(UnmanagedType.LPStr)] string text, int length);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Resource_CreateSoundFromFile(byte* path);
	}
}

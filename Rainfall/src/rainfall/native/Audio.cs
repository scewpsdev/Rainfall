using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Rainfall.Native
{
	internal static class Audio
	{
		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_Init();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_Shutdown();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_ListenerUpdateTransform(Vector3 position, Vector3 forward, Vector3 up);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint Audio_CreateSource(Vector3 position);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_DestroySource(uint source);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceUpdateTransform(uint source, Vector3 position);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourcePlaySound(uint source, IntPtr sound, float gain, float pitch);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceStop(uint source);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourcePause(uint source);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceResume(uint source);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceRewind(uint source);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetGain(uint source, float gain);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetPitch(uint source, float pitch);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetLooping(uint source, bool looping);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetAmbientMode(uint source, bool ambient);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern float Audio_SoundGetDuration(IntPtr sound);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SetEffectNone();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SetEffectReverb();
	}
}

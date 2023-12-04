using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public enum AudioEffect
	{
		None,
		Reverb,
	}

	public static class Audio
	{
		public static void Init()
		{
			Native.Audio.Audio_Init();
		}

		public static void Shutdown()
		{
			Native.Audio.Audio_Shutdown();
		}

		public static AudioSource CreateSource(Vector3 position)
		{
			uint handle = Native.Audio.Audio_CreateSource(position);
			if (handle != 0)
				return new AudioSource(handle);
			return null;
		}

		public static void DestroySource(AudioSource source)
		{
			Native.Audio.Audio_DestroySource(source.handle);
		}

		public static void UpdateListener(Vector3 position, Quaternion rotation)
		{
			Native.Audio.Audio_ListenerUpdateTransform(position, rotation.forward, rotation.up);
		}

		public static void SetEffect(AudioEffect effect)
		{
			if (effect == AudioEffect.Reverb)
				Native.Audio.Audio_SetEffectReverb();
			else
				Native.Audio.Audio_SetEffectNone();
		}
	}
}

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

		public static void UpdateListener(Vector3 position, Quaternion rotation)
		{
			Native.Audio.Audio_ListenerUpdateTransform(position, rotation.forward, rotation.up);
		}

		public static uint PlayBackground(Sound sound, float gain = 1.0f, float pitch = 1.0f)
		{
			return Native.Audio.Audio_PlayBackground(sound.handle, gain, pitch, 0);
		}

		public static uint PlayBackgroundLooping(Sound sound, float gain = 1.0f, float pitch = 1.0f)
		{
			return Native.Audio.Audio_PlayBackground(sound.handle, gain, pitch, 1);
		}

		public static void SetSourceGain(uint source, float gain)
		{
			Native.Audio.Audio_SourceSetGain(source, gain);
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

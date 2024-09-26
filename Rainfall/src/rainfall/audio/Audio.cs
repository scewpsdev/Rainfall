using Rainfall;
using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

		public static void Update()
		{
			Native.Audio.Audio_Update();
		}

		public static void SetGlobalVolume(float volume)
		{
			Audio_SetGlobalVolume(volume);
		}

		public static void Set3DVolume(float volume)
		{
			Audio_Set3DVolume(volume);
		}

		public static void UpdateListener(Vector3 position, Quaternion rotation)
		{
			Native.Audio.Audio_ListenerUpdateTransform(position, rotation.forward, rotation.up);
		}

		public static uint Play(Sound sound, Vector3 position, float gain = 1, float pitch = 1, float rolloff = 0.5f)
		{
			return Native.Audio.Audio_SourcePlay(sound.handle, position, gain, pitch, rolloff);
		}

		public static uint Play(Sound[] sounds, Vector3 position, float gain = 1, float pitch = 1, float rolloff = 0.5f)
		{
			return Play(sounds[Random.Shared.Next() % sounds.Length], position, gain, pitch, rolloff);
		}

		public static uint PlayOrganic(Sound sound, Vector3 position, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f, float rolloff = 0.5f)
		{
			float gainFactor = MathHelper.RandomFloat(1.0f - gainVariation, 1.0f + gainVariation);
			float pitchFactor = MathHelper.RandomFloat(1.0f - pitchVariation, 1.0f + pitchVariation);
			return Native.Audio.Audio_SourcePlay(sound.handle, position, gainFactor * gain, pitchFactor * pitch, rolloff);
		}

		public static uint PlayOrganic(Sound[] sounds, Vector3 position, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f, float rolloff = 0.5f)
		{
			return PlayOrganic(sounds[Random.Shared.Next() % sounds.Length], position, gain, pitch, gainVariation, pitchVariation, rolloff);
		}

		public static uint PlayBackground(Sound sound, float gain = 1.0f, float pitch = 1.0f, bool looping = false, float fadein = 0)
		{
			return Native.Audio.Audio_PlayBackground(sound.handle, gain, pitch, (byte)(looping ? 1 : 0), fadein);
		}

		public static void PlayBackground(Sound[] sounds, float gain = 1.0f, float pitch = 1.0f, bool looping = false, float fadein = 0)
		{
			PlayBackground(sounds[Random.Shared.Next() % sounds.Length], gain, pitch, looping, fadein);
		}

		public static void Stop(uint source)
		{
			Native.Audio.Audio_SourceStop(source);
		}

		public static void SetPaused(uint source, bool paused)
		{
			Native.Audio.Audio_SourceSetPaused(source, (byte)(paused ? 1 : 0));
		}

		public static void SetSourcePosition(uint source, Vector3 position)
		{
			Native.Audio.Audio_SourceSetPosition(source, position);
		}

		public static void SetSourceGain(uint source, float gain)
		{
			Native.Audio.Audio_SourceSetGain(source, gain);
		}

		public static void SetSourceLooping(uint source, bool looping)
		{
			Native.Audio.Audio_SourceSetLooping(source, (byte)(looping ? 1 : 0));
		}

		public static void StopSource(uint source)
		{
			Native.Audio.Audio_SourceStop(source);
		}

		public static void FadeoutSource(uint source, float time)
		{
			Native.Audio.Audio_SourceFadeout(source, time);
		}

		public static void FadeoutVolume(uint source, float time)
		{
			Native.Audio.Audio_SourceFadeoutVolume(source, time);
		}

		public static void FadeinVolume(uint source, float time)
		{
			Native.Audio.Audio_SourceFadeinVolume(source, time);
		}

		public static void SetEffect(AudioEffect effect)
		{
			if (effect == AudioEffect.Reverb)
				Native.Audio.Audio_SetEffectReverb();
			else
				Native.Audio.Audio_SetEffectNone();
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SetGlobalVolume(float volume);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_Set3DVolume(float volume);
	}
}

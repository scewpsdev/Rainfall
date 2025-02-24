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
			Audio_Init();
		}

		public static void Shutdown()
		{
			Audio_Shutdown();
		}

		public static void Update()
		{
			Audio_Update();
		}

		public static void SetGlobalVolume(float volume)
		{
			Audio_SetGlobalVolume(volume);
		}

		public static void Set3DVolume(float volume)
		{
			Audio_Set3DVolume(volume);
		}

		public static void SetMusicVolume(float volume)
		{
			Audio_SetMusicVolume(volume);
		}

		public static void UpdateListener(Vector3 position, Quaternion rotation)
		{
			Audio_ListenerUpdateTransform(position, rotation.forward, rotation.up);
		}

		public static void SetListenerVelocity(Vector3 velocity)
		{
			Audio_ListenerSetVelocity(velocity);
		}

		public static uint Play(Sound sound, Vector3 position, float gain = 1, float pitch = 1, float rolloff = 0.5f)
		{
			return Audio_SourcePlay(sound.handle, Time.deltaTime, position, gain, pitch, rolloff);
		}

		public static uint Play(Sound[] sounds, Vector3 position, float gain = 1, float pitch = 1, float rolloff = 0.5f)
		{
			return Play(sounds[Random.Shared.Next() % sounds.Length], position, gain, pitch, rolloff);
		}

		public static uint PlayOrganic(Sound sound, Vector3 position, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f, float rolloff = 0.5f)
		{
			float gainFactor = MathHelper.RandomFloat(1.0f - gainVariation, 1.0f + gainVariation);
			float pitchFactor = MathHelper.RandomFloat(1.0f - pitchVariation, 1.0f + pitchVariation);
			return Audio_SourcePlay(sound.handle, Time.deltaTime, position, gainFactor * gain, pitchFactor * pitch, rolloff);
		}

		public static uint PlayOrganic(Sound[] sounds, Vector3 position, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.0f, float pitchVariation = 0.25f, float rolloff = 0.5f)
		{
			return PlayOrganic(sounds[Random.Shared.Next() % sounds.Length], position, gain, pitch, gainVariation, pitchVariation, rolloff);
		}

		public static uint PlayBackground(Sound sound, float gain = 1.0f, float pitch = 1.0f, bool looping = false, float fadein = 0)
		{
			return Audio_SourcePlayBackground(Resource.Resource_SoundGetHandle(sound.resource), gain, pitch, (byte)(looping ? 1 : 0), fadein);
		}

		public static uint PlayBackgroundClocked(Sound sound, float gain = 1.0f, float pitch = 1.0f, bool looping = false, float fadein = 0)
		{
			return Audio_SourcePlayBackgroundClocked(Resource.Resource_SoundGetHandle(sound.resource), Time.deltaTime, gain, pitch, (byte)(looping ? 1 : 0), fadein);
		}

		public static uint PlayBackground(Sound[] sounds, float gain = 1.0f, float pitch = 1.0f, bool looping = false, float fadein = 0)
		{
			return PlayBackground(sounds[Random.Shared.Next() % sounds.Length], gain, pitch, looping, fadein);
		}

		public static uint PlayBackgroundOrganic(Sound sound, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.05f)
		{
			float gainFactor = MathHelper.RandomFloat(1.0f - gainVariation, 1.0f + gainVariation);
			float pitchFactor = MathHelper.RandomFloat(1.0f - pitchVariation, 1.0f + pitchVariation);
			return Audio_SourcePlayBackground(Resource.Resource_SoundGetHandle(sound.resource), gainFactor * gain, pitchFactor * pitch, 0, 0);
		}

		public static uint PlayBackgroundOrganic(Sound[] sounds, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f)
		{
			return PlayBackgroundOrganic(sounds[Random.Shared.Next() % sounds.Length], gain, pitch, gainVariation, pitchVariation);
		}

		public static uint PlayMusic(Sound sound, float gain = 1.0f, bool looping = false, float fadein = 0)
		{
			return Audio_SourcePlayMusic(Resource.Resource_SoundGetHandle(sound.resource), gain, (byte)(looping ? 1 : 0), fadein);
		}

		public static void SetPaused(uint source, bool paused)
		{
			Audio_SourceSetPaused(source, (byte)(paused ? 1 : 0));
		}

		public static void SetSourcePosition(uint source, Vector3 position)
		{
			Audio_SourceSetPosition(source, position);
		}

		public static void SetSourceVelocity(uint source, Vector3 velocity)
		{
			Audio_SourceSetVelocity(source, velocity);
		}

		public static void SetSourceGain(uint source, float gain)
		{
			Audio_SourceSetGain(source, gain);
		}

		public static void SetSourcePitch(uint source, float pitch)
		{
			Audio_SourceSetPitch(source, pitch);
		}

		public static void SetSourceLooping(uint source, bool looping)
		{
			Audio_SourceSetLooping(source, (byte)(looping ? 1 : 0));
		}

		public static void StopSource(uint source)
		{
			Audio_SourceStop(source);
		}

		public static void FadeoutSource(uint source, float time)
		{
			Audio_SourceFadeout(source, time);
		}

		public static void FadeoutVolume(uint source, float time)
		{
			Audio_SourceFadeoutVolume(source, time);
		}

		public static void FadeinVolume(uint source, float time)
		{
			Audio_SourceFadeinVolume(source, time);
		}

		public static void FadeVolume(uint source, float volume, float time)
		{
			Audio_SourceFadeVolume(source, volume, time);
		}

		public static void SetInaudibleBehavior(uint source, bool mustTick, bool kill)
		{
			Audio_SourceSetInaudibleBehavior(source, (byte)(mustTick ? 1 : 0), (byte)(kill ? 1 : 0));
		}

		public static void SetProtect(uint source, bool protect)
		{
			Audio_SourceSetProtect(source, (byte)(protect ? 1 : 0));
		}

		public static void SetEffect(AudioEffect effect)
		{
			if (effect == AudioEffect.Reverb)
				Audio_SetEffectReverb();
			else
				Audio_SetEffectNone();
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SetGlobalVolume(float volume);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_Set3DVolume(float volume);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SetMusicVolume(float volume);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_Init();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_Shutdown();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_Update();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_ListenerUpdateTransform(Vector3 position, Vector3 forward, Vector3 up);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_ListenerSetVelocity(Vector3 velocity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint Audio_SourcePlayBackground(IntPtr sound, float gain, float pitch, byte looping, float fadein);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint Audio_SourcePlayBackgroundClocked(IntPtr sound, float deltaTime, float gain, float pitch, byte looping, float fadein);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint Audio_SourcePlayMusic(IntPtr sound, float gain, byte looping, float fadein);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint Audio_SourcePlay(IntPtr sound, float deltaTime, Vector3 position, float gain, float pitch, float rolloff);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceStop(uint source);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetPaused(uint source, byte paused);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourcePause(uint source);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceResume(uint source);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceRewind(uint source);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceFadeout(uint source, float time);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceFadeoutVolume(uint source, float time);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceFadeinVolume(uint source, float time);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceFadeVolume(uint source, float volume, float time);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetPosition(uint source, Vector3 position);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetVelocity(uint source, Vector3 velocity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetGain(uint source, float gain);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetPitch(uint source, float pitch);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetLooping(uint source, byte looping);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetInaudibleBehavior(uint source, byte mustTick, byte kill);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SourceSetProtect(uint source, byte protect);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SoundSetSingleInstance(IntPtr sound, byte singleInstance);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SetEffectNone();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Audio_SetEffectReverb();
	}
}

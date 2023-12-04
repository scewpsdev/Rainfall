using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class AudioSource
	{
		internal uint handle;

		Sound currentSound = null;
		long lastPlayed = 0;


		internal AudioSource(uint handle)
		{
			this.handle = handle;
		}

		public void updateTransform(Vector3 position)
		{
			Native.Audio.Audio_SourceUpdateTransform(handle, position);
		}

		public void playSound(Sound sound, float gain = 1.0f, float pitch = 1.0f)
		{
			Native.Audio.Audio_SourcePlaySound(handle, sound.handle, gain, pitch);
			currentSound = sound;
			lastPlayed = Time.currentTime;
		}

		public void playSoundOrganic(Sound sound, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f)
		{
			float gainFactor = MathHelper.RandomFloat(1.0f - gainVariation, 1.0f + gainVariation);
			float pitchFactor = MathHelper.RandomFloat(1.0f - pitchVariation, 1.0f + pitchVariation);
			Native.Audio.Audio_SourcePlaySound(handle, sound.handle, gainFactor * gain, pitchFactor * pitch);
		}

		public void playSoundOrganic(Span<Sound> sound, float gain = 1.0f, float pitch = 1.0f)
		{
			playSoundOrganic(sound[Random.Shared.Next() % sound.Length], gain, pitch);
		}

		public void stop()
		{
			Native.Audio.Audio_SourceStop(handle);
		}

		public void pause()
		{
			Native.Audio.Audio_SourcePause(handle);
		}

		public void resume()
		{
			Native.Audio.Audio_SourceResume(handle);
		}

		public void rewind()
		{
			Native.Audio.Audio_SourceRewind(handle);
		}

		public float gain
		{
			set { Native.Audio.Audio_SourceSetGain(handle, value); }
		}

		public float pitch
		{
			set { Native.Audio.Audio_SourceSetPitch(handle, value); }
		}

		public bool isLooping
		{
			set { Native.Audio.Audio_SourceSetLooping(handle, value); }
		}

		public bool isAmbient
		{
			set { Native.Audio.Audio_SourceSetAmbientMode(handle, value); }
		}

		public bool isPlaying
		{
			get { return (Time.currentTime - lastPlayed) / 1e9f < currentSound.duration; }
		}
	}
}

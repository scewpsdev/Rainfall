using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class AudioSource
	{
		uint source;
		Vector3 position;

		public Sound currentSound { get; private set; } = null;
		long lastPlayed = 0;


		public AudioSource(Vector3 position)
		{
			this.position = position;
		}

		public void destroy()
		{

		}

		public void updateTransform(Vector3 position)
		{
			this.position = position;
			if (source != 0)
				Native.Audio.Audio_SourceSetPosition(source, position);
		}

		public void playSound(Sound sound, float gain = 1.0f, float pitch = 1.0f)
		{
			source = Native.Audio.Audio_SourcePlay(sound.handle, position, gain, pitch);
			currentSound = sound;
			lastPlayed = Time.currentTime;
		}

		public void playSoundOrganic(Sound sound, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f)
		{
			float gainFactor = MathHelper.RandomFloat(1.0f - gainVariation, 1.0f + gainVariation);
			float pitchFactor = MathHelper.RandomFloat(1.0f - pitchVariation, 1.0f + pitchVariation);
			source = Native.Audio.Audio_SourcePlay(sound.handle, position, gainFactor * gain, pitchFactor * pitch);
			currentSound = sound;
			lastPlayed = Time.currentTime;
		}

		public void playSoundOrganic(Span<Sound> sound, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f)
		{
			playSoundOrganic(sound[Random.Shared.Next() % sound.Length], gain, pitch, gainVariation, pitchVariation);
		}

		public void stop()
		{
			Native.Audio.Audio_SourceStop(source);
			currentSound = null;
			lastPlayed = 0;
		}

		public void pause()
		{
			Native.Audio.Audio_SourcePause(source);
		}

		public void resume()
		{
			Native.Audio.Audio_SourceResume(source);
		}

		public void rewind()
		{
			Native.Audio.Audio_SourceRewind(source);
		}

		public float gain
		{
			set { Native.Audio.Audio_SourceSetGain(source, value); }
		}

		public float pitch
		{
			set { Native.Audio.Audio_SourceSetPitch(source, value); }
		}

		public bool isLooping
		{
			set { Native.Audio.Audio_SourceSetLooping(source, (byte)(value ? 1 : 0)); }
		}

		public float timePlaying
		{
			get => (Time.currentTime - lastPlayed) / 1e9f;
		}

		public bool isPlaying
		{
			get { return currentSound != null && (Time.currentTime - lastPlayed) / 1e9f < currentSound.duration; }
		}
	}
}

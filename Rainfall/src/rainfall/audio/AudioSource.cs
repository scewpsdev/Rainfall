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
				Audio.Audio_SourceSetPosition(source, position);
		}

		public void playSound(Sound sound, float gain = 1.0f, float pitch = 1.0f, float rolloff = 0.2f)
		{
			source = Audio.Audio_SourcePlay(sound.handle, Time.deltaTime, position, gain, pitch, rolloff);
			currentSound = sound;
			lastPlayed = Time.currentTime;
		}

		public void playSoundOrganic(Sound sound, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f, float rolloff = 0.2f)
		{
			float gainFactor = MathHelper.RandomFloat(1.0f - gainVariation, 1.0f + gainVariation);
			float pitchFactor = MathHelper.RandomFloat(1.0f - pitchVariation, 1.0f + pitchVariation);
			source = Audio.Audio_SourcePlay(sound.handle, Time.deltaTime, position, gainFactor * gain, pitchFactor * pitch, rolloff);
			currentSound = sound;
			lastPlayed = Time.currentTime;
		}

		public void playSoundOrganic(Span<Sound> sound, float gain = 1.0f, float pitch = 1.0f, float gainVariation = 0.2f, float pitchVariation = 0.25f, float rolloff = 0.2f)
		{
			playSoundOrganic(sound[Random.Shared.Next() % sound.Length], gain, pitch, gainVariation, pitchVariation, rolloff);
		}

		public void stop()
		{
			Audio.Audio_SourceStop(source);
			currentSound = null;
			lastPlayed = 0;
		}

		public void pause()
		{
			Audio.Audio_SourcePause(source);
		}

		public void resume()
		{
			Audio.Audio_SourceResume(source);
		}

		public void rewind()
		{
			Audio.Audio_SourceRewind(source);
		}

		public float gain
		{
			set { Audio.Audio_SourceSetGain(source, value); }
		}

		public float pitch
		{
			set { Audio.Audio_SourceSetPitch(source, value); }
		}

		public bool isLooping
		{
			set { Audio.Audio_SourceSetLooping(source, (byte)(value ? 1 : 0)); }
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

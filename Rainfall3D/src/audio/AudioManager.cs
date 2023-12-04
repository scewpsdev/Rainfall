using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class AudioManager
{
	static AudioSource ambientSource;
	static Sound ambientSound;

	static bool reverb = false;
	static float ambientSoundGainDst = 0.0f;
	static float ambientSoundGain = 0.0f;


	public static void Init()
	{
		ambientSource = Audio.CreateSource(Vector3.Zero);
		ambientSource.isAmbient = true;
		ambientSource.isLooping = true;
	}

	public static void Update()
	{
		float gainMultiplier = reverb ? 0.2f : 1.0f;
		ambientSoundGain = MathHelper.Lerp(ambientSoundGain, ambientSoundGainDst * gainMultiplier, 2.0f * Time.deltaTime);
		ambientSource.gain = ambientSoundGain;
	}

	public static void SetAmbientSound(Sound sound, float gain = 1.0f)
	{
		ambientSound = sound;
		ambientSource.playSound(ambientSound, 0.0f, 1.0f);
		ambientSoundGainDst = gain;
	}

	public static void SetReverb(bool reverb)
	{
		if (reverb)
		{
			Audio.SetEffect(AudioEffect.Reverb);
		}
		else
		{
			Audio.SetEffect(AudioEffect.None);
		}
		AudioManager.reverb = reverb;
	}
}

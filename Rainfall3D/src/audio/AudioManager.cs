using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class AudioManager
{
	static uint ambientSource;
	public static Sound currentAmbientSound { get; private set; }
	public static float currentAmbientGain { get; private set; }


	public static void Init()
	{
	}

	public static void Update()
	{
		/*
		float gainMultiplier = reverb ? 0.2f : 1.0f;
		ambientSoundGain = MathHelper.Lerp(ambientSoundGain, ambientSoundGainDst * gainMultiplier, 2.0f * Time.deltaTime);
		if (ambientSource != 0)
			Audio.SetSourceGain(ambientSource, ambientSoundGain);
		*/
	}

	public static void SetAmbientSound(Sound sound, float gain = 1.0f)
	{
		if (ambientSource != 0)
			Audio.FadeoutSource(ambientSource, 4.0f);
		ambientSource = Audio.PlayBackground(sound, gain, 1, true, 4.0f);
		currentAmbientSound = sound;
		currentAmbientGain = gain;
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
	}
}

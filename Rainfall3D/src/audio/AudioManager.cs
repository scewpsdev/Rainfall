using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class AudioManager
{
	static bool reverb = false;
	static float ambientSoundGainDst = 0.0f;
	static float ambientSoundGain = 0.0f;

	static uint ambientSource;


	public static void Init()
	{
	}

	public static void Update()
	{
		float gainMultiplier = reverb ? 0.2f : 1.0f;
		ambientSoundGain = MathHelper.Lerp(ambientSoundGain, ambientSoundGainDst * gainMultiplier, 2.0f * Time.deltaTime);
		Audio.SetSourceGain(ambientSource, ambientSoundGain);
	}

	public static void SetAmbientSound(Sound sound, float gain = 1.0f)
	{
		ambientSource = Audio.PlayBackgroundLooping(sound, gain);
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

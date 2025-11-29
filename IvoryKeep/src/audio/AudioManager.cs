using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class AudioManager
{
	public static Sound ambience { get; private set; }
	static uint ambienceSource;


	public static void SetAmbience(Sound ambience)
	{
		if (AudioManager.ambience != ambience)
		{
			if (ambienceSource != 0)
			{
				Audio.FadeoutSource(ambienceSource, 2);
				ambienceSource = 0;
			}
			if (ambience != null)
			{
				ambienceSource = Audio.PlayBackground(ambience, 0.4f, 1, true, 2);
				Audio.SetInaudibleBehavior(ambienceSource, true, false);
				Audio.SetProtect(ambienceSource, true);
			}
			AudioManager.ambience = ambience;
		}
	}
}

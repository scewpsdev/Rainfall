using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public static class AudioManager
{
	public static Sound ambience { get; private set; }
	static uint ambienceSource;

	static MultilayerTrack ambientTrack;
	static bool ambientHasIdle;


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

	public static void SetAmbientTrack(MultilayerTrack track, bool hasIdleLayer)
	{
		if (ambientTrack != track)
		{
			if (ambientTrack != null)
				ambientTrack.stop();

			if (track != null && hasIdleLayer)
			{
				track.start();
				track.setLayer(0);
			}

			ambientTrack = track;
			ambientHasIdle = hasIdleLayer;
		}
	}

	public static void SetAmbientTrackLayer(int layer)
	{
		if (ambientTrack != null)
		{
			if (layer >= 0 && !ambientTrack.running)
				ambientTrack.start();
			ambientTrack.setLayer(layer + (ambientHasIdle ? 1 : 0));
		}
	}
}

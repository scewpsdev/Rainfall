using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SoundtrackManager
{
	static uint source1, source2, source3;

	static Sound ost1, ost2, ost3;

	public static void Init()
	{
		ost1 = Resource.GetSound("sounds/ost/ost1.ogg");
		ost2 = Resource.GetSound("sounds/ost/ost2.ogg");
		ost3 = Resource.GetSound("sounds/ost/ost3.ogg");
		source1 = Audio.PlayBackground(ost1, 0);
		source2 = Audio.PlayBackground(ost2, 0);
		source3 = Audio.PlayBackground(ost3, 0);
		Audio.SetSourceLooping(source1, true);
		Audio.SetSourceLooping(source2, true);
		Audio.SetSourceLooping(source3, true);
		Audio.SetInaudibleBehavior(source1, true, false);
		Audio.SetInaudibleBehavior(source2, true, false);
		Audio.SetInaudibleBehavior(source3, true, false);
	}

	public static void SetLayer(int layer)
	{
		const float fadeout = 3;
		const float fadein = 5;

		const float volume = 0.25f;

		if (layer == 0)
		{
			Audio.FadeoutVolume(source1, fadeout);
			Audio.FadeoutVolume(source2, fadeout);
			Audio.FadeoutVolume(source3, fadeout);
		}
		else if (layer == 1)
		{
			Audio.FadeVolume(source1, volume, fadein);
			Audio.FadeoutVolume(source2, fadeout);
			Audio.FadeoutVolume(source3, fadeout);
		}
		else if (layer == 2)
		{
			Audio.FadeoutVolume(source1, fadeout);
			Audio.FadeVolume(source2, volume, fadein);
			Audio.FadeoutVolume(source3, fadeout);
		}
		else if (layer == 3)
		{
			Audio.FadeoutVolume(source1, fadeout);
			Audio.FadeoutVolume(source2, fadeout);
			Audio.FadeVolume(source3, volume, fadein);
		}
	}
}
